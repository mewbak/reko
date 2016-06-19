﻿#region License
/* 
 * Copyright (C) 1999-2016 John Källén.
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
using Reko.Core;
using Reko.Core.Expressions;
using Reko.Core.Machine;
using Reko.Core.Types;

namespace Reko.Arch.Vax
{
    public class MemoryOperand : MachineOperand
    {
        public MemoryOperand(PrimitiveType width) : base(width) { }

        public RegisterStorage Base;
        public Constant Offset;
        public bool Deferred;
        internal bool AutoDecrement;
        internal bool AutoIncrement;
        internal RegisterStorage Index;

        public override void Write(bool fExplicit, MachineInstructionWriter writer)
        {
            if (Offset != null)
            {
                writer.Write(FormatSignedValue(Offset));
            }
            if (AutoDecrement)
            {
                writer.Write("-");
            }
            if (Base != null)
            {
                writer.Write("({0})", Base.Name);
            }
            if (AutoIncrement)
            {
                writer.Write("+");
            }
            if (Index != null)
            {
                writer.Write("[{0}]", Index.Name);
            }
        }
    }
}