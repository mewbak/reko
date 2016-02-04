﻿using Reko.Core;
using Reko.Core.Expressions;
using Reko.Core.Lib;
using Reko.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reko.Scanning
{
    public class VectorWorkItem : WorkItem
    {
        public ImageMapVectorTable Table;
        public ProcessorState State;
        public Address AddrFrom;			// address from which the jump is called.
        public PrimitiveType Stride;
        public ushort SegBase;

        private IScanner scanner;
        private Program program;
        private Procedure proc;
        private Dictionary<Address, VectorUse> vectorUses;
        private bool isCallTable;

        public VectorWorkItem(IScanner scanner, Program program, ImageMapVectorTable table, bool isCallTable, Procedure proc)
            : base(table.Address)
        {
            this.scanner = scanner;
            this.program = program;
            this.Table = table;
            this.proc = proc;
            this.isCallTable = isCallTable;
            this.vectorUses = new Dictionary<Address, VectorUse>();
        }

        public override void Process()
        {
            var builder = new VectorBuilder(scanner,  program, new DirectedGraphImpl<object>());
            var vector = builder.Build(Table.TableAddress, AddrFrom, State);
            if (vector.Count == 0)
            {
                Address addrNext = Table.TableAddress + Stride.Size;
                if (program.ImageMap.IsValidAddress(addrNext))
                {
                    // Can't determine the size of the table, but surely it has one entry?
                   program.ImageMap.AddItem(addrNext, new ImageMapItem());
                }
                return;
            }

            Table.Addresses.AddRange(vector);
            for (int i = 0; i < vector.Count; ++i)
            {
                var st = State.Clone();
                if (isCallTable)
                {
                    scanner.ScanProcedure(vector[i], null, st);
                }
                else
                {
                    scanner.EnqueueJumpTarget(AddrFrom, vector[i], proc, st);
                }
            }
            vectorUses[AddrFrom] = new VectorUse(Table.TableAddress, builder.IndexRegister);
            program.ImageMap.AddItem(Table.TableAddress + builder.TableByteSize, new ImageMapItem());
        }
    }
}
