﻿#region License
/* 
 * Copyright (C) 1999-2010 John Källén.
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
#endregion

using System;

namespace Decompiler.Core.Code
{
    /// <summary>
    /// This class models both unconditional jumps and conditional jumps. The 
    /// rewriting phase converts these to Branch instructions as appropriate.
    /// </summary>
    public class GotoInstruction : Instruction
    {
        private Expression condition;
        private Expression target;

        /// <summary>
        /// Use this constructor to create an unconditional transfer instruction.
        /// </summary>
        /// <param name="target">The destination, either as a linear address or as an expression.</param>
        public GotoInstruction(Expression target)
        {
            this.target = target;
            this.condition = Constant.Invalid;
        }

        /// <summary>
        /// Use this constructor to crate a conditional branch instruction.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="target"></param>
        public GotoInstruction(Expression condition, Expression target)
        {
            this.condition = condition;
            this.target = target;
        }

        public override Instruction Accept(InstructionTransformer xform)
        {
            return xform.TransformGotoInstruction(this);
        }

        public override void Accept(InstructionVisitor v)
        {
            v.VisitGotoInstruction(this);
        }

        public bool IsConditional { get { return Condition != Constant.Invalid; } }

        public override bool IsControlFlow { get { return true; } }

        public Expression Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        /// <summary>
        /// The target of the goto instruction. Either a Constant, in which case it should 
        /// be an address, or 
        /// </summary>
        public Expression Target
        {
            get { return target; }
            set { target = value; }
        }
    }
}