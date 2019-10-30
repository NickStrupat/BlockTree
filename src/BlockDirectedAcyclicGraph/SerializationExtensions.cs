using System;
using System.Buffers.Binary;

namespace NickStrupat
{
    static class SerializationExtensions
	{
		public static Int32 GetSerializationLength(this ReadOnlySpan<Byte> bytes) => sizeof(Int32) + bytes.Length;

		public static void Serialize(this ReadOnlySpan<Byte> bytes, Span<Byte> destination)
		{
			BinaryPrimitives.WriteInt32BigEndian(destination, bytes.Length);
			bytes.CopyTo(destination.Slice(sizeof(Int32)));
		}

		public static void CopyFromAndAdvance(ref this Span<Byte> buffer, ReadOnlySpan<Byte> source)
		{
			source.Serialize(buffer);
			buffer = buffer.Slice(source.GetSerializationLength());
		}

		public static void CopyFromAndAdvance(ref this Span<Byte> buffer, ImmutableSpan<Byte> source) => buffer.CopyFromAndAdvance(source.Span);
		public static void CopyFromAndAdvance(ref this Span<Byte> buffer, ReadOnlyMemory<Byte> source) => buffer.CopyFromAndAdvance(source.Span);
		public static void CopyFromAndAdvance(ref this Span<Byte> buffer, ImmutableMemory<Byte> source) => buffer.CopyFromAndAdvance(source.Memory);

		public static ImmutableSpan<Byte> ReadAndAdvance(ref this ImmutableSpan<Byte> buffer)
		{
			var length = BinaryPrimitives.ReadInt32BigEndian(buffer.Span);
			var extracted = buffer.Slice(sizeof(Int32), length);
			buffer = buffer.Slice(sizeof(Int32) + length);
			return extracted;
		}

		public static ImmutableMemory<Byte> ReadAndAdvance(ref this ImmutableMemory<Byte> buffer)
		{
			var length = BinaryPrimitives.ReadInt32BigEndian(buffer.ImmutableSpan.Span);
			var extracted = buffer.Slice(sizeof(Int32), length);
			buffer = buffer.Slice(sizeof(Int32) + length);
			return extracted;
		}
	}
}
