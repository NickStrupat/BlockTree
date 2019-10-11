using System;

namespace TheBlockTree
{
	public struct Bytes : IEquatable<Bytes>
	{
		public static Bytes Empty { get; } = Array.Empty<Byte>();

		public readonly Byte[] Value;
		public Bytes(Byte[] value) => Value = value;
		public override String ToString() => Convert.ToBase64String(Value);

		public Boolean Equals(Bytes other) => ByteArrayEquals(Value, other.Value);
		public override Boolean Equals(Object obj) => obj is Bytes bytes && Value.Equals(bytes);
		public override Int32 GetHashCode() => Value.Length.GetHashCode();

		public static Boolean operator ==(Bytes left, Bytes right) => left.Equals(right);
		public static Boolean operator !=(Bytes left, Bytes right) => !(left == right);

		public Int32 Length => Value.Length;

		public static implicit operator Bytes(Byte[] value) => new Bytes(value);
		public static implicit operator Byte[](Bytes bytes) => bytes.Value;

		private static Boolean ByteArrayEquals(ReadOnlySpan<Byte> a, ReadOnlySpan<Byte> b) =>
			a.Length == b.Length && (a == b || a.SequenceEqual(b));
	}
}
