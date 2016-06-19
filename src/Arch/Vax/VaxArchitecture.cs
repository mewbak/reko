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

using Reko.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reko.Core.Expressions;
using Reko.Core.Machine;
using Reko.Core.Rtl;
using Reko.Core.Types;
using System.Globalization;

namespace Reko.Arch.Vax
{
    public class VaxArchitecture : ProcessorArchitecture
    {
        private static RegisterStorage[] regs = new[]
        {
            Registers.r0,
            Registers.r1,
            Registers.r2,
            Registers.r3,

            Registers.r4,
            Registers.r5,
            Registers.r6,
            Registers.r7,

            Registers.r8,
            Registers.r9,
            Registers.r10,
            Registers.r11,

            Registers.ap,
            Registers.fp,
            Registers.sp,
            Registers.pc,
        };

        public VaxArchitecture()
        {
            InstructionBitSize = 8;
            this.FramePointerType = PrimitiveType.Pointer32;
            this.WordWidth = PrimitiveType.Word32;
            this.PointerType = PrimitiveType.Pointer32;
        }

        public override IEnumerable<MachineInstruction> CreateDisassembler(ImageReader imageReader)
        {
            return new VaxDisassembler(this, imageReader);
        }

        public override ImageReader CreateImageReader(MemoryArea img, ulong off)
        {
            return new LeImageReader(img, off);
        }

        public override ImageReader CreateImageReader(MemoryArea img, Address addr)
        {
            return new LeImageReader(img, addr);
        }

        public override ImageReader CreateImageReader(MemoryArea img, Address addrBegin, Address addrEnd)
        {
            return new LeImageReader(img, addrBegin, addrEnd);
        }

        public override ImageWriter CreateImageWriter()
        {
            return new LeImageWriter();
        }

        public override ImageWriter CreateImageWriter(MemoryArea mem, Address addr)
        {
            return new LeImageWriter(mem, addr);
        }

        public override IEqualityComparer<MachineInstruction> CreateInstructionComparer(Normalize norm)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Address> CreatePointerScanner(SegmentMap map, ImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags)
        {
            throw new NotImplementedException();
        }

        public override ProcessorState CreateProcessorState()
        {
            return new VaxProcessorState(this);
        }

        public override IEnumerable<RtlInstructionCluster> CreateRewriter(ImageReader rdr, ProcessorState state, Frame frame, IRewriterHost host)
        {
            return new VaxRewriter(this, rdr, state, frame, host);
        }

        public override Expression CreateStackAccess(Frame frame, int cbOffset, DataType dataType)
        {
            throw new NotImplementedException();
        }

        public override FlagGroupStorage GetFlagGroup(string name)
        {
            throw new NotImplementedException();
        }

        public override FlagGroupStorage GetFlagGroup(uint grf)
        {
            throw new NotImplementedException();
        }

        public override RegisterStorage GetRegister(string name)
        {
            throw new NotImplementedException();
        }

        public override RegisterStorage GetRegister(int i)
        {
            return regs[i];
        }

        public override RegisterStorage[] GetRegisters()
        {
            throw new NotImplementedException();
        }

        //$REVIEW: shouldn't this be flaggroup?
        private static RegisterStorage[] flagRegisters = {
            new RegisterStorage("C", 0, 0, PrimitiveType.Bool),
            new RegisterStorage("V", 0, 0, PrimitiveType.Bool),
            new RegisterStorage("Z", 0, 0, PrimitiveType.Bool),
            new RegisterStorage("N", 0, 0, PrimitiveType.Bool),
        };

        public override string GrfToString(uint grf)
        {
            StringBuilder s = new StringBuilder();
            for (int r = 0; grf != 0; ++r, grf >>= 1)
            {
                if ((grf & 1) != 0)
                    s.Append(flagRegisters[r].Name);
            }
            return s.ToString();
        }

        public override Address MakeAddressFromConstant(Constant c)
        {
            return Address.Ptr32(c.ToUInt32());
        }

        public override Address ReadCodeAddress(int size, ImageReader rdr, ProcessorState state)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetRegister(string name, out RegisterStorage reg)
        {
            throw new NotImplementedException();
        }

        public override bool TryParseAddress(string txtAddr, out Address addr)
        {
            uint uAddr;
            if (!uint.TryParse(txtAddr, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uAddr))
            {
                addr = null;
                return false;
            }
            else
            {
                addr = Address.Ptr32(uAddr);
                return true;
            }
        }
    }
}