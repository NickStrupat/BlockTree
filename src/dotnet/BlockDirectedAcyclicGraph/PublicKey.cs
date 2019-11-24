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

		public Int32 SerializationLength =>
			sizeof(SignatureAlgorithmCode) +
			sizeof(Int32) +
			Bytes.Length;

		public void SerializeTo(Span<Byte> destination) => SerializeToAndAdvance(ref destination);
		public void SerializeToAndAdvance(ref Span<Byte> destination)
		{
			destination.WriteEnumAndAdvance(SignatureAlgorithmCode);
			destination.WriteInt32AndAdvance(Bytes.Length);
			destination.WriteBytesAndAdvance(Bytes);
		}

		public static PublicKey DeserializeFrom(ImmutableMemory<Byte> source) => DeserializeFromAndAdvance(ref source);
		public static PublicKey DeserializeFromAndAdvance(ref ImmutableMemory<Byte> source)
		{
			var signatureAlgorithmCode = (SignatureAlgorithmCode)source.ReadByteAndAdvance();
			var signatureAlgorithm = signatureAlgorithmCode.GetSignatureAlgorithm();
			var publicKeyLength = source.ReadInt32AndAdvance();
			if (publicKeyLength != signatureAlgorithm.PublicKeySize)
				throw new InvalidPublicKeyLengthException(signatureAlgorithm.PublicKeySize, publicKeyLength);
			var signatureBytes = source.ReadBytesAndAdvance(publicKeyLength);
			return new PublicKey(signatureAlgorithmCode, signatureBytes);
		}

		public override Int32 GetHashCode() => ImmutableMemoryEqualityComparer<Byte>.Instance.GetHashCode(Bytes);
		public override Boolean Equals(Object o) => o is PublicKey pk && Equals(pk);
		public Boolean Equals(PublicKey other) =>
			SignatureAlgorithmCode == other.SignatureAlgorithmCode &&
			ImmutableMemoryEqualityComparer<Byte>.Instance.Equals(Bytes, other.Bytes);
	}
}
