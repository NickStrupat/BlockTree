﻿using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NickStrupat
{
	public partial class Block
	{
		public readonly PublicKey PublicKey;
		public readonly ImmutableMemory<Signature> ParentSignatures;
		public readonly ImmutableMemory<Byte> Data;
		public readonly Signature Signature;

		public Block(IEnumerable<Block> parentBlocks, ImmutableMemory<Byte> data, Key key)
		{
			PublicKey = key.PublicKey;
			ParentSignatures = GetParentSignatures(parentBlocks);
			Data = data;
			Signature = new Signature(key, SignParentSignaturesAndData);
		}

		private ImmutableMemory<Signature> GetParentSignatures(IEnumerable<Block> parentBlocks)
		{
			return ImmutableMemory<Signature>.Create(parentBlocks.Count(), parentBlocks, spanAction);
			static void spanAction(Span<Signature> span, IEnumerable<Block> parentsBlocks)
			{
				var i = 0;
				foreach (var parentBlock in parentsBlocks)
					span[i] = parentBlock.Signature;
			}
		}

		private Int32 LengthOfCryptoBytes => ParentSignatures.AsSpan().GetSerializationLength() + Data.Length);

		private void CopyBytesForCryptoTo(Span<Byte> destination)
		{
			foreach (var parentSignature in ParentSignatures.AsImmutableSpan())
			{
				parentSignature.SerializeTo(destination);
				destination = destination.Slice(parentSignature.SerializationLength);
			}
			Data.AsSpan().CopyTo(destination);
		}

		private void SignParentSignaturesAndData(Span<Byte> signature, Key key)
		{
			Span<byte> bytesForCrypto = stackalloc Byte[LengthOfCryptoBytes];
			CopyBytesForCryptoTo(bytesForCrypto);
			SignatureAlgorithm.Sign(key, bytesForCrypto, signature);
		}

		private Boolean VerifyParentSignaturesAndData()
		{
			Span<byte> bytesForCrypto = stackalloc Byte[LengthOfCryptoBytes];
			CopyBytesForCryptoTo(bytesForCrypto);
			return SignatureAlgorithm.Verify(PublicKey, bytesForCrypto, Signature.Bytes.AsSpan());
		}

		private static Boolean IsParentSignaturesLengthValid(Int32 parentSignaturesLength) =>
			parentSignaturesLength % (sizeof(Int32) + SignatureAlgorithm.SignatureSize) == 0;

		public static readonly SignatureAlgorithm SignatureAlgorithm = SignatureAlgorithm.Ed25519;
		public static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;
	}
}
