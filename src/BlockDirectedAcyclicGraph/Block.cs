using NSec.Cryptography;
using System;

namespace NickStrupat
{
	public readonly partial struct Block
	{
		public readonly ImmutableMemory<Byte> ParentSignatures;
		public readonly ImmutableMemory<Byte> Data;
		public readonly ImmutableMemory<Byte> Signature;
		public readonly ImmutableMemory<Byte> PublicKey;

		public SignaturesEnumerable ParentSignaturesEnumerable => new SignaturesEnumerable(ParentSignatures, Algorithm.SignatureSize);

		public Block(ImmutableMemory<Byte> parentSignatures, ImmutableMemory<Byte> data, Key key) : this()
		{
			if (!IsParentSignaturesLengthValid(parentSignatures))
				throw new ArgumentException();
			ParentSignatures = parentSignatures;
			Data = data;
			PublicKey = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
			Signature = SignParentSignaturesAndData(key);
		}

		private Int32 LengthOfCryptoBytes => ParentSignatures.Length + Data.Length;

		private void CopyBytesForCryptoTo(Span<Byte> destination)
		{
			var buffer = destination;
			foreach (var parentSignature in ParentSignaturesEnumerable)
			{
				parentSignature.ImmutableSpan.CopyTo(buffer);
				buffer = buffer.Slice(parentSignature.Length);
			}
			Data.ImmutableSpan.CopyTo(buffer);
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
			if (!NSec.Cryptography.PublicKey.TryImport(Algorithm, PublicKey.ImmutableSpan.Span, PublicKeyBlobFormat, out var publicKey))
				return false;
			return Algorithm.Verify(publicKey, bytesForCrypto, Signature.ImmutableSpan.Span);
		}

		public Boolean Verify() => VerifyParentSignaturesAndData();

		private static Boolean IsParentSignaturesLengthValid(ImmutableMemory<Byte> parentSignatures) =>
			parentSignatures.Length % Algorithm.SignatureSize == 0;

		public static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;
		public static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;
	}
}
