using System;
using System.Runtime.InteropServices;

namespace TheBlockTree
{
    static class ReadOnlySpanExtensions
	{
		public static Int32 GetSerializationLength(this ReadOnlySpan<Byte> bytes) => sizeof(Int32) + bytes.Length;
		public static void Serialize(this ReadOnlySpan<Byte> bytes, Span<Byte> destination)
		{
			MemoryMarshal.AsBytes(stackalloc Int32[] { bytes.Length }).CopyTo(destination);
			bytes.CopyTo(destination.Slice(sizeof(Int32)));
		}
	}
}
