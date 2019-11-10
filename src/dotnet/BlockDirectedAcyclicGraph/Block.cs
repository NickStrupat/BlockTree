using NSec.Cryptography;
using System;

namespace NickStrupat
{
	public readonly partial struct Block
	{
		public readonly UInt32 Version;
		public readonly SignatureAlgorithmCode SignatureAlgorithmCode;
		public readonly ImmutableMemory<Byte> PublicKey;
		public readonly ImmutableMemory<Byte> ParentSignatures;
		public readonly ImmutableMemory<Byte> Data;
		public readonly ImmutableMemory<Byte> Nonce;
		public readonly ImmutableMemory<Byte> Signature;

		public SignaturesEnumerable ParentSignaturesEnumerable => new SignaturesEnumerable(ParentSignatures, Algorithm.SignatureSize);

		public Block(ImmutableMemory<Byte> parentSignatures, ImmutableMemory<Byte> data, Key key) : this()
		{
			Version = 1;
			SignatureAlgorithmCode = SignatureAlgorithmCode.Ed25519;
			if (!IsParentSignaturesLengthValid(parentSignatures.Length))
				throw new InvalidParentSignaturesLengthException(parentSignatures.Length);
			PublicKey = new ImmutableMemory<Byte>(key.PublicKey.Export(KeyBlobFormat.RawPublicKey));
			ParentSignatures = parentSignatures;
			Data = data;
			Nonce = ImmutableMemory<Byte>.Create((Int32) NonceByteLength, 0, (span, _) => RandomGenerator.Default.GenerateBytes(span));
			Signature = new ImmutableMemory<Byte>(SignParentSignaturesAndData(key));
		}

		private Int32 LengthOfCryptoBytes => ParentSignatures.Length + Data.Length + checked((Int32) NonceByteLength);

		private void CopyBytesForCryptoTo(Span<Byte> destination)
		{
			var buffer = destination;
			foreach (var parentSignature in ParentSignaturesEnumerable)
			{
				parentSignature.AsSpan().CopyTo(buffer);
				buffer = buffer.Slice(parentSignature.Length);
			}
			Data.AsSpan().CopyTo(buffer);
			buffer = buffer.Slice(Data.Length);
			Nonce.AsSpan().CopyTo(buffer);
		}

		private Byte[] SignParentSignaturesAndData(Key key)
		{
			Span<byte> bytesForCrypto = stackalloc Byte[LengthOfCryptoBytes];
			CopyBytesForCryptoTo(bytesForCrypto);
			return Algorithm.Sign(key, bytesForCrypto);
		}

		private Boolean VerifyParentSignaturesAndData()
		{
			Span<byte> bytesForCrypto = stackalloc Byte[LengthOfCryptoBytes];
			CopyBytesForCryptoTo(bytesForCrypto);
			if (!NSec.Cryptography.PublicKey.TryImport(Algorithm, PublicKey.AsSpan(), PublicKeyBlobFormat, out var publicKey))
				return false;
			return Algorithm.Verify(publicKey, bytesForCrypto, Signature.AsSpan());
		}

		public Boolean Verify() => VerifyParentSignaturesAndData();

		private static Boolean IsParentSignaturesLengthValid(Int32 parentSignaturesLength) =>
			parentSignaturesLength % Algorithm.SignatureSize == 0;

		public static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;
		public static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;
		private const UInt32 NonceByteLength = 32;
	}

	public enum SignatureAlgorithmCode : UInt32 { Ed25519 = 0 }
}
