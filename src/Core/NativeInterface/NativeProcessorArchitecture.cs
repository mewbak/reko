﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reko.Core.Expressions;
using Reko.Core.Machine;
using Reko.Core.Rtl;
using Reko.Core.Types;
using Reko.Core.NativeInterface.Interfaces;
using System.Runtime.InteropServices;

namespace Reko.Core.NativeInterface
{
	public class NativeProcessorArchitecture : ProcessorArchitecture
	{
		private IntPtr handle;
		private ILibraryLoader loader;
		private NativeProcessorArchitectureProvider prv;

		private const string SYM_NAME = "gCPUProvider";

		public NativeProcessorArchitecture(string archID, string libPath, ILibraryLoader ldr) : base(archID)
		{
			loader = ldr;
			handle = ldr.LoadLibrary(libPath);
			prv = (NativeProcessorArchitectureProvider)Marshal.PtrToStructure(ldr.GetSymbol(handle, SYM_NAME), typeof(NativeProcessorArchitectureProvider));
		}

		public new string Name {
			get {
				return prv.GetCpuName();
			}
		}

		public new string Description {
			get {
				return prv.GetDescription();
			}
		}

		public override IEnumerable<MachineInstruction> CreateDisassembler(EndianImageReader imageReader)
		{
			throw new NotImplementedException();
		}

		public override EndianImageReader CreateImageReader(MemoryArea img, Address addr)
		{
			throw new NotImplementedException();
		}

		public override EndianImageReader CreateImageReader(MemoryArea img, Address addrBegin, Address addrEnd)
		{
			throw new NotImplementedException();
		}

		public override EndianImageReader CreateImageReader(MemoryArea img, ulong off)
		{
			throw new NotImplementedException();
		}

		public override ImageWriter CreateImageWriter()
		{
			throw new NotImplementedException();
		}

		public override ImageWriter CreateImageWriter(MemoryArea img, Address addr)
		{
			throw new NotImplementedException();
		}

		public override IEqualityComparer<MachineInstruction> CreateInstructionComparer(Normalize norm)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<Address> CreatePointerScanner(SegmentMap map, EndianImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags)
		{
			throw new NotImplementedException();
		}

		public override ProcessorState CreateProcessorState(SegmentMap segmentMap)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<RtlInstructionCluster> CreateRewriter(EndianImageReader rdr, ProcessorState state, IStorageBinder frame, IRewriterHost host)
		{
			throw new NotImplementedException();
		}

		public override Expression CreateStackAccess(IStorageBinder frame, int cbOffset, DataType dataType)
		{
			throw new NotImplementedException();
		}

		public override FlagGroupStorage GetFlagGroup(uint grf)
		{
			throw new NotImplementedException();
		}

		public override FlagGroupStorage GetFlagGroup(string name)
		{
			throw new NotImplementedException();
		}

		public override SortedList<string, int> GetOpcodeNames()
		{
			throw new NotImplementedException();
		}

		public override int? GetOpcodeNumber(string name)
		{
			throw new NotImplementedException();
		}

		public override RegisterStorage GetRegister(int i)
		{
			throw new NotImplementedException();
		}

		public override RegisterStorage GetRegister(string name)
		{
			throw new NotImplementedException();
		}

		public override RegisterStorage[] GetRegisters()
		{
			throw new NotImplementedException();
		}

		public override string GrfToString(uint grf)
		{
			throw new NotImplementedException();
		}

		public override Address MakeAddressFromConstant(Constant c)
		{
			throw new NotImplementedException();
		}

		public override Address ReadCodeAddress(int size, EndianImageReader rdr, ProcessorState state)
		{
			throw new NotImplementedException();
		}

		public override bool TryGetRegister(string name, out RegisterStorage reg)
		{
			throw new NotImplementedException();
		}

		public override bool TryParseAddress(string txtAddr, out Address addr)
		{
			throw new NotImplementedException();
		}

        public override bool TryRead(MemoryArea mem, Address addr, PrimitiveType dt, out Constant value)
        {
            throw new NotImplementedException();
        }
    }
}
