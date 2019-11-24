using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace NickStrupat
{
    static class SerializationExtensions
	{
		public static void WriteByteAndAdvance(ref this Span<Byte> buffer, Byte value)
		{
			buffer[0] = value;
			buffer = buffer.Slice(sizeof(Byte));
		}

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


		public static Byte ReadByteAndAdvance(ref this ReadOnlySpan<Byte> buffer)
		{
			var @byte = buffer[0];
			buffer = buffer.Slice(sizeof(Byte));
			return @byte;
		}


		public static Byte ReadByteAndAdvance(ref this ImmutableSpan<Byte> buffer)
		{
			var @byte = buffer[0];
			buffer = buffer.Slice(sizeof(Byte));
			return @byte;
		}


		public static Byte ReadByteAndAdvance(ref this ImmutableMemory<Byte> buffer)
		{
			var @byte = buffer.AsSpan()[0];
			buffer = buffer.Slice(sizeof(Byte));
			return @byte;
		}

		public static Int32 ReadInt32AndAdvance(ref this ReadOnlySpan<Byte> buffer)
		{
			var length = BinaryPrimitives.ReadInt32BigEndian(buffer);
			buffer = buffer.Slice(sizeof(Int32));
			return length;
		}

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

		public static ReadOnlySpan<Byte> ReadBytesAndAdvance(ref this ReadOnlySpan<Byte> buffer, Int32 length)
		{
			var extracted = buffer.Slice(0, length);
			buffer = buffer.Slice(length);
			return extracted;
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

		private delegate void GetWriter(Span<Byte> buffer, Object value);
		private delegate Object GetReader(ReadOnlySpan<Byte> buffer);
		private static readonly Dictionary<Type, (GetWriter writerGetter, GetReader readerGetter, Int32 size)> enumInfos = new Dictionary<Type, (GetWriter, GetReader, Int32)>
		{
			[typeof(Byte)] = ((span, o) => span[0] = (Byte)o, span => span[0], sizeof(Byte)),
			[typeof(Int16)] = ((span, o) => BinaryPrimitives.WriteInt16BigEndian(span, (Int16)o), span => BinaryPrimitives.ReadInt16BigEndian(span), sizeof(Int16)),
			[typeof(Int32)] = ((span, o) => BinaryPrimitives.WriteInt32BigEndian(span, (Int32)o), span => BinaryPrimitives.ReadInt32BigEndian(span), sizeof(Int32)),
			[typeof(Int64)] = ((span, o) => BinaryPrimitives.WriteInt64BigEndian(span, (Int64)o), span => BinaryPrimitives.ReadInt64BigEndian(span), sizeof(Int64)),
			[typeof(UInt16)] = ((span, o) => BinaryPrimitives.WriteUInt16BigEndian(span, (UInt16)o), span => BinaryPrimitives.ReadUInt16BigEndian(span), sizeof(UInt16)),
			[typeof(UInt32)] = ((span, o) => BinaryPrimitives.WriteUInt32BigEndian(span, (UInt32)o), span => BinaryPrimitives.ReadUInt32BigEndian(span), sizeof(UInt32)),
			[typeof(UInt64)] = ((span, o) => BinaryPrimitives.WriteUInt64BigEndian(span, (UInt64)o), span => BinaryPrimitives.ReadUInt64BigEndian(span), sizeof(UInt64)),
		};

		public static void WriteEnumAndAdvance<T>(ref this Span<Byte> buffer, T value) where T : unmanaged, Enum
		{
			var underlyingType = Enum.GetUnderlyingType(typeof(T));
			var (writerGetter, readerGetter, size) = enumInfos[underlyingType];
			writerGetter.Invoke(buffer, value);
			buffer = buffer.Slice(size);
		}

		public static T ReadEnumAndAdvance<T>(ref this ReadOnlySpan<Byte> buffer) where T : unmanaged, Enum
		{
			var underlyingType = Enum.GetUnderlyingType(typeof(T));
			var (writerGetter, readerGetter, size) = enumInfos[underlyingType];
			var value = (T)readerGetter.Invoke(buffer);
			buffer = buffer.Slice(size);
			return value;
		}

		public static T ReadEnumAndAdvance<T>(ref this ImmutableSpan<Byte> buffer) where T : unmanaged, Enum
		{
			var underlyingType = Enum.GetUnderlyingType(typeof(T));
			var (writerGetter, readerGetter, size) = enumInfos[underlyingType];
			var value = (T)readerGetter.Invoke(buffer.AsSpan());
			buffer = buffer.Slice(size);
			return value;
		}

		public static T ReadEnumAndAdvance<T>(ref this ImmutableMemory<Byte> buffer) where T : unmanaged, Enum
		{
			var underlyingType = Enum.GetUnderlyingType(typeof(T));
			var (writerGetter, readerGetter, size) = enumInfos[underlyingType];
			var value = (T)readerGetter.Invoke(buffer.AsSpan());
			buffer = buffer.Slice(size);
			return value;
		}

		public static void ReadEnumAndAdvance<T>(ref this ReadOnlySpan<Byte> buffer, out T value) where T : unmanaged, Enum =>
			value = buffer.ReadEnumAndAdvance<T>();

		public static void ReadEnumAndAdvance<T>(ref this ImmutableSpan<Byte> buffer, out T value) where T : unmanaged, Enum =>
			value = buffer.ReadEnumAndAdvance<T>();

		public static void ReadEnumAndAdvance<T>(ref this ImmutableMemory<Byte> buffer, out T value) where T : unmanaged, Enum =>
			value = buffer.ReadEnumAndAdvance<T>();
	}
}
