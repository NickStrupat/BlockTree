using NSec.Cryptography;
using System;

namespace NickStrupat
{
	public readonly struct PublicKey : IEquatable<PublicKey>
	{
		public readonly SignatureAlgorithmCode SignatureAlgorithmCode;
		public readonly ImmutableMemory<Byte> Bytes;

		public PublicKey(NSec.Cryptography.PublicKey publicKey)
		{
			if (!(publicKey.Algorithm is SignatureAlgorithm sa))
				throw new InvalidAlgorithmException(publicKey.Algorithm);
			SignatureAlgorithmCode = sa.GetSignatureAlgorithm();
			Bytes = publicKey.Export(KeyBlobFormat.RawPublicKey).ToImmutableMemory();
		}

		private PublicKey(SignatureAlgorithmCode signatureAlgorithmCode, ImmutableMemory<Byte> bytes) =>
			(SignatureAlgorithmCode, Bytes) = (signatureAlgorithmCode, bytes);

		public Boolean Verify(ReadOnlySpan<Byte> data, ReadOnlySpan<Byte> signature) =>
			SignatureAlgorithmCode.Verify(Bytes.AsSpan(), data, signature);

		public Int32 SerializationLength => sizeof(SignatureAlgorithmCode) + Bytes.Length;

		public void SerializeTo(Span<Byte> destination) => SerializeToAndAdvance(ref destination);
		public void SerializeToAndAdvance(ref Span<Byte> destination)
		{
			destination.WriteInt32AndAdvance((Int32)SignatureAlgorithmCode);
			destination.WriteBytesAndAdvance(Bytes);
		}

		public static PublicKey DeserializeFrom(ImmutableMemory<Byte> source) => DeserializeFromAndAdvance(ref source);
		public static PublicKey DeserializeFromAndAdvance(ref ImmutableMemory<Byte> source)
		{
			var signatureAlgorithmCode = (SignatureAlgorithmCode)source.ReadInt32AndAdvance();
			var signatureAlgorithm = signatureAlgorithmCode.GetSignatureAlgorithm();
			var signatureBytes = source.ReadBytesAndAdvance(signatureAlgorithm.SignatureSize);
			return new PublicKey(signatureAlgorithmCode, signatureBytes);
		}

		public override Int32 GetHashCode() => ImmutableMemoryEqualityComparer<Byte>.Instance.GetHashCode(Bytes);
		public override Boolean Equals(Object o) => o is PublicKey pk && Equals(pk);
		public Boolean Equals(PublicKey other) =>
			SignatureAlgorithmCode == other.SignatureAlgorithmCode &&
			ImmutableMemoryEqualityComparer<Byte>.Instance.Equals(Bytes, other.Bytes);
	}
}
