using System;

namespace TheBlockTree
{
	public struct Bytes : IEquatable<Bytes>
	{
		public static Bytes Empty { get; } = ReadOnlyMemory<Byte>.Empty;

		public readonly ReadOnlyMemory<Byte> Value;
		public Bytes(ReadOnlyMemory<Byte> value) => Value = value;
		public override String ToString() => Convert.ToBase64String(Value.Span);

		public Boolean Equals(Bytes other) => ByteArrayEquals(Value.Span, other.Value.Span);
		public override Boolean Equals(Object obj) => obj is Bytes bytes && this.Equals(bytes);
		public override Int32 GetHashCode() => Value.Length.GetHashCode();

		public static Boolean operator ==(Bytes left, Bytes right) => left.Equals(right);
		public static Boolean operator !=(Bytes left, Bytes right) => !(left == right);

		public Int32 Length => Value.Length;

		public static implicit operator Bytes(ReadOnlyMemory<Byte> value) => new Bytes(value);
		public static implicit operator ReadOnlyMemory<Byte>(Bytes bytes) => bytes.Value;

		private static Boolean ByteArrayEquals(ReadOnlySpan<Byte> a, ReadOnlySpan<Byte> b) =>
			a.Length == b.Length && (a == b || a.SequenceEqual(b));
	}
}
