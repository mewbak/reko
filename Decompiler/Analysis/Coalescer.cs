/* 
 * Copyright (C) 1999-2009 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Decompiler.Analysis
{
	/// <summary>
	/// Builds expression trees out of identifier assignment statements and 
	/// moves assignment statements (def's) as close to their 
	/// uses in this block as possible. 
	/// </summary>
    /// <remarks>
    /// This transformation is quite destabiling and ineffective if long chains of expressions are still in their
    /// 3-address format. Before coalescing, call ValuePropagation to perform constant propagation and 
    /// other transformations that make the expression trees smaller.
    /// </remarks>
	public class Coalescer : InstructionTransformer
	{
		private Procedure proc;
		private SsaState ssa;
		private SideEffectFinder sef;
		private bool coalesced;
        private Dictionary<Statement, List<SsaIdentifier>> defsByStatement;

		private static TraceSwitch trace = new TraceSwitch("Coalescer", "Traces the progress of identifier coalescing");

		public Coalescer(Procedure proc, SsaState ssa)
		{
			this.proc = proc;
			this.ssa = ssa;
			this.sef = new SideEffectFinder();
            this.defsByStatement = new Dictionary<Statement, List<SsaIdentifier>>();
            foreach (SsaIdentifier sid in ssa.Identifiers)
            {
                if (sid.DefStatement != null)
                    SetDefStatement(sid.DefStatement, sid);
            }
		}

        private void SetDefStatement(Statement stm, SsaIdentifier sid)
        {
            List<SsaIdentifier> sids;
            if (defsByStatement.TryGetValue(sid.DefStatement, out sids))
            {
                sids.Remove(sid);
            }
            if (!defsByStatement.TryGetValue(stm, out sids))
            {
                sids = new List<SsaIdentifier>();
                defsByStatement.Add(stm, sids);
            }
            sids.Add(sid);
        }

        /// <summary>
        /// Returns true if the identifer <paramref name="sid"/>, which is defined in <paramref name="def"/>, can safely
        /// be coalesced into <paramref name="use"/>.
        /// </summary>
        /// <param name="sid">identifier common to <paramref name="def"/> and <paramref name="use"/>.</param>
        /// <param name="def">Statement that defines <paramref name="sid"/>. </param>
        /// <param name="use">Statement that uses <paramref name="sid"/>. </param>
        /// <returns></returns>
		public bool CanCoalesce(SsaIdentifier sid, Statement def, Statement use)
		{
			if (sid.Uses.Count != 1)
				return false;
			System.Diagnostics.Debug.Assert(sid.Uses[0] == use);
			if (use.Instruction is PhiAssignment)
				return false;
			if (use.Instruction is UseInstruction)
				return false;

            //$PERFORMANCE: this loop might be slow and should be improved if possible.
            List<SsaIdentifier> sids;
            if (defsByStatement.TryGetValue(def, out sids))
            {
                foreach (SsaIdentifier sidOther in sids)
                {
                    if (sidOther != sid && sidOther.IsSideEffect)
                    {
                        if (sidOther.Uses.Contains(use))
                            return false;
                    }
                }
            }
			return true;
		}

		public bool Coalesced
		{
			get { return coalesced; }
			set { coalesced = value; }
		}

		/// <summary>
		/// Coalesces the single use and the single definition of an identifier.
		/// </summary>
		/// <param name="sid"></param>
		/// <param name="defExpr"></param>
		/// <param name="def"></param>
		/// <param name="use"></param>
		/// <returns></returns>
		public bool CoalesceStatements(SsaIdentifier sid, Expression defExpr, Statement def, Statement use)
		{
			if (trace.TraceInfo)
			{
				Debug.WriteLineIf(trace.TraceInfo, "Coalescing on " + sid.Identifier.ToString());
				Debug.Indent();
				Debug.WriteLineIf(trace.TraceInfo, def.Instruction.ToString());
				Debug.WriteLineIf(trace.TraceInfo, use.Instruction.ToString());
				Debug.Unindent();
			}
			UsedIdentifierAdjuster uia = new UsedIdentifierAdjuster(def, ssa.Identifiers, use);
			def.Instruction.Accept(uia);
			IdentifierReplacer ir = new IdentifierReplacer(sid.Identifier, defExpr);
			use.Instruction.Accept(ir);

            List<SsaIdentifier> sids;
            if (defsByStatement.TryGetValue(def, out sids))
            {
                foreach (SsaIdentifier s in sids)
                {
                    if (s != sid)
                    {
                        s.DefStatement = use;
                        SetDefStatement(use, s);
                    }
                }
            }
			ssa.DeleteStatement(def);

			if (trace.TraceInfo)
			{
				Debug.WriteLineIf(trace.TraceInfo, "  ; coalesced to");
				Debug.Indent();
				Debug.WriteLineIf(trace.TraceInfo, use.Instruction.ToString());
				Debug.Unindent();
			}
			return true;
		}

		private bool MoveAssignment(int initPos, int newPos, Block block)
		{
			if (initPos + 1 == newPos)
				return false;
			block.Statements.Insert(newPos, block.Statements[initPos]);
			block.Statements.RemoveAt(initPos);
			return true;
		}

		public void Process(Block block)
		{
			do
			{
				Coalesced = false;

				bool[] visited = new bool[ssa.Identifiers.Count];
				for (int i = 0; i < block.Statements.Count; ++i)
				{
					Statement stmDef = block.Statements[i];
					Assignment ass = stmDef.Instruction as Assignment;
					if (ass != null && !visited[ass.Dst.Number])
					{
						visited[ass.Dst.Number] = true;
						SsaIdentifier sidDef = ssa.Identifiers[ass.Dst];
						if (TryMoveAssignment(stmDef, sidDef, ass.Src, block, i))
						{
							--i;
						}
					}
				}
			} while (Coalesced);
		}

		public void Transform()
		{
			foreach (Block b in proc.RpoBlocks)
			{
				Process(b);
			}
		}


		/// <summary>
		/// Tries to move the assigment as far down the block as is possible.
		/// </summary>
		/// <param name="ass"></param>
		/// <param name="block"></param>
		/// <param name="i"></param>
		/// <returns>true if a change was made</returns>
		public bool TryMoveAssignment(Statement stmDef, SsaIdentifier sidDef, Expression defExpr, Block block, int initialPosition)
		{
			SideEffectFlags flagsDef = sef.FindSideEffect(stmDef.Instruction);
			for (int i = initialPosition + 1; i < block.Statements.Count; ++i)
			{
				Statement stm = block.Statements[i];
				if (sidDef.Uses.Contains(stm))
				{
					if (CanCoalesce(sidDef, stmDef, stm))
					{
						Coalesced = true;
						return CoalesceStatements(sidDef, defExpr, stmDef, stm);
					}
					else
					{
						return MoveAssignment(initialPosition, i, block);
					}
				}
				if (stm.Instruction.IsControlFlow)
				{
					return MoveAssignment(initialPosition, i, block);
				}

				SideEffectFlags flagsStm = sef.FindSideEffect(stm.Instruction);
				if (sef.Conflict(flagsDef, flagsStm))
				{
					return MoveAssignment(initialPosition, i, block);
				}
			}
			return MoveAssignment(initialPosition, block.Statements.Count, block);
		}
    }

		/// <summary>
		/// Replaces all occurences of an identifier with another identifier.
		/// </summary>
    public class IdentifierReplacer : InstructionTransformer
    {
        private Identifier idOld;
        private Expression defExpr;

        public IdentifierReplacer(Identifier idOld, Expression defExpr)
        {
            this.idOld = idOld;
            this.defExpr = defExpr;
        }

        public override Expression TransformIdentifier(Identifier id)
        {
            if (idOld == id)
                return defExpr;
            else
                return id;
        }

    }

    /// <summary>
    /// Replace uses of identifiers in the defined statement to reflect that it
    /// has been moved into the using statement.
    /// </summary>
    public class UsedIdentifierAdjuster : InstructionVisitorBase
    {
        private Statement def;
        private Statement use;
        private SsaIdentifierCollection ssaIds;

        public UsedIdentifierAdjuster(Statement def, SsaIdentifierCollection ssaIds, Statement use)
        {
            this.def = def;
            this.use = use;
            this.ssaIds = ssaIds;
        }

        public void Transform()
        {
            def.Instruction.Accept(this);
        }

        public override void VisitAssignment(Assignment a)
        {
            a.Src.Accept(this);
        }

        public override void VisitIdentifier(Identifier id)
        {
            SsaIdentifier sid = ssaIds[id];
            for (int i = 0; i < sid.Uses.Count; ++i)
            {
                if (sid.Uses[i] == def)
                    sid.Uses[i] = use;
            }
        }

        public override void VisitStore(Store store)
        {
            store.Dst.Accept(this);
            store.Src.Accept(this);
        }
    }
}