using System;
using System.Buffers.Binary;

namespace NickStrupat
{
    static class SerializationExtensions
	{
		public static void WriteInt32AndAdvance(ref this Span<Byte> buffer, Int32 value)
		{
			BinaryPrimitives.WriteInt32BigEndian(buffer, value);
			buffer = buffer.Slice(sizeof(Int32));
		}

		public static void WriteBytesAndAdvance(ref this Span<Byte> buffer, ReadOnlySpan<Byte> source)
		{
			source.CopyTo(buffer);
			buffer = buffer.Slice(source.Length);
		}

		public static void WriteBytesAndAdvance(ref this Span<Byte> buffer, ImmutableSpan<Byte> source) => buffer.WriteBytesAndAdvance(source.AsSpan());
		public static void WriteBytesAndAdvance(ref this Span<Byte> buffer, ReadOnlyMemory<Byte> source) => buffer.WriteBytesAndAdvance(source.Span);
		public static void WriteBytesAndAdvance(ref this Span<Byte> buffer, ImmutableMemory<Byte> source) => buffer.WriteBytesAndAdvance(source.AsMemory());


		public static Int32 ReadInt32AndAdvance(ref this ImmutableSpan<Byte> buffer)
		{
			var length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan());
			buffer = buffer.Slice(sizeof(Int32));
			return length;
		}

		public static Int32 ReadInt32AndAdvance(ref this ImmutableMemory<Byte> buffer)
		{
			var length = BinaryPrimitives.ReadInt32BigEndian(buffer.AsSpan());
			buffer = buffer.Slice(sizeof(Int32));
			return length;
		}

		public static ImmutableSpan<Byte> ReadBytesAndAdvance(ref this ImmutableSpan<Byte> buffer, Int32 length)
		{
			var extracted = buffer.Slice(0, length);
			buffer = buffer.Slice(length);
			return extracted;
		}

		public static ImmutableMemory<Byte> ReadBytesAndAdvance(ref this ImmutableMemory<Byte> buffer, Int32 length)
		{
			var extracted = buffer.Slice(0, length);
			buffer = buffer.Slice(length);
			return extracted;
		}
	}
}
