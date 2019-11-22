using NSec.Cryptography;
using System;
using System.Buffers;

namespace NickStrupat
{
	public readonly struct Signature : IEquatable<Signature>
	{
		public readonly SignatureAlgorithmCode SignatureAlgorithmCode;
		public readonly ImmutableMemory<Byte> Bytes;

		public Signature(Key key, SpanAction<Byte, Key> spanAction)
		{
			if (!(key.Algorithm is SignatureAlgorithm signatureAlgorithm))
				throw new InvalidAlgorithmException(key.Algorithm);
			SignatureAlgorithmCode = signatureAlgorithm.GetSignatureAlgorithm();
			Bytes = ImmutableMemory<Byte>.Create(signatureAlgorithm.SignatureSize, key, spanAction);
		}

		private Signature(SignatureAlgorithmCode signatureAlgorithmCode, ImmutableMemory<Byte> bytes)
		{
			SignatureAlgorithmCode = signatureAlgorithmCode;
			Bytes = bytes;
		}

		public Int32 SerializationLength => sizeof(SignatureAlgorithmCode) + Bytes.Length;

		public void SerializeTo(Span<Byte> destination) => SerializeToAndAdvance(ref destination);

		public void SerializeToAndAdvance(ref Span<Byte> destination)
		{
			destination.WriteInt32AndAdvance((Int32)SignatureAlgorithmCode);
			destination.WriteBytesAndAdvance(Bytes);
		}

		public static Signature DeserializeFrom(ImmutableMemory<Byte> source)
		{
			var signatureAlgorithmCode = (SignatureAlgorithmCode)source.ReadInt32AndAdvance();
			var signatureAlgorithm = signatureAlgorithmCode.GetSignatureAlgorithm();
			var signatureBytes = source.ReadBytesAndAdvance(signatureAlgorithm.SignatureSize);
			return new Signature(signatureAlgorithmCode, signatureBytes);
		}

		public override Boolean Equals(Object obj) => obj is Signature signature && Equals(signature);

		public Boolean Equals(Signature other) =>
			SignatureAlgorithmCode == other.SignatureAlgorithmCode &&
			Bytes.Length == other.Bytes.Length &&
			Bytes.Equals(other.Bytes);

		public override Int32 GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(SignatureAlgorithmCode);
			foreach (var @byte in Bytes.AsImmutableSpan())
				hashCode.Add(@byte);
			return hashCode.ToHashCode();
		}

		public static Boolean operator ==(Signature left, Signature right) =>
			left.SignatureAlgorithmCode == right.SignatureAlgorithmCode &&
			left.Bytes.AsSpan() == right.Bytes.AsSpan();
		public static Boolean operator !=(Signature left, Signature right) => !(left == right);
	}
}
