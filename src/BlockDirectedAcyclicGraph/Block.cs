using NSec.Cryptography;
using System;

namespace BlockTree
{
	public readonly partial struct Block
	{
		public SignatureEnumerable ParentSignatures => new SignatureEnumerable(parentSignaturesBytes, Algorithm.SignatureSize);
		private readonly ImmutableMemory<Byte> parentSignaturesBytes;
		public readonly ImmutableMemory<Byte> Data;
		public readonly ImmutableMemory<Byte> Signature;
		public readonly ImmutableMemory<Byte> PublicKey;

		public Block(ImmutableMemory<Byte> parentSignatures, ImmutableMemory<Byte> data, Key key) : this()
		{
			if (parentSignatures.Length == 0 || parentSignatures.Length % Algorithm.SignatureSize != 0)
				throw new ArgumentException();
			this.parentSignaturesBytes = parentSignatures;
			Data = data;
			PublicKey = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
			Signature = SignParentSignaturesAndData(key);
		}

		private Int32 LengthOfCryptoBytes => parentSignaturesBytes.Length + Data.Length;

		private void CopyBytesForCryptoTo(Span<Byte> destination)
		{
			var buffer = destination;
			foreach (var parentSignature in ParentSignatures)
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
			var publicKey = NSec.Cryptography.PublicKey.Import(Algorithm, PublicKey.ImmutableSpan.Span, PublicKeyBlobFormat);
			return Algorithm.Verify(publicKey, bytesForCrypto, Signature.ImmutableSpan.Span);
		}

		public Boolean Verify(Func<ImmutableMemory<Byte>, Block> getBlockBySignature) // FUCK, how do i verify multiple parents
		{
			// simple check if signatures match
			foreach (var parentSignature in ParentSignatures)
			{
				var parentBlock = getBlockBySignature(parentSignature);
				if (!parentBlock.Signature.ImmutableSpan.Span.SequenceEqual(parentSignature.ImmutableSpan.Span))
					return false;
			}

			// actual data integrity check that the signature of (parent signature + data) results in the child's signature
			return child.VerifyParentSignatureAndData();
		}

		private static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;
		private static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;
	}
}
