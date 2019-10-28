using System;
using System.Buffers.Binary;

namespace BlockTree
{
    static class ReadOnlySpanExtensions
	{
		public static Int32 GetSerializationLength(this ReadOnlySpan<Byte> bytes) => sizeof(Int32) + bytes.Length;
		public static void Serialize(this ReadOnlySpan<Byte> bytes, Span<Byte> destination)
		{
			BinaryPrimitives.WriteInt32BigEndian(destination, bytes.Length);
			bytes.CopyTo(destination.Slice(sizeof(Int32)));
		}
	}
}
