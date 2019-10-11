using System;
using NSec.Cryptography;

namespace TheBlockTree
{
	public class Block
    {
		public Bytes ParentSignature { get; }
		public Bytes Data { get; }
		public Bytes Signature { get; }
		public Bytes PublicKey { get; }

		internal Block(Byte[] parentSignature, Byte[] data, Key key)
		{
			ParentSignature = parentSignature;
			Data = data;
			PublicKey = key.PublicKey.Export(PublicKeyBlobFormat);
			Signature = SignParentSignatureAndData(key);
		}

		private Byte[] SignParentSignatureAndData(Key key)
		{
			Span<Byte> data = stackalloc Byte[ParentSignature.Length + Data.Length];
			ParentSignature.Value.CopyTo(data);
			Data.Value.CopyTo(data.Slice(ParentSignature.Length));
			return Algorithm.Sign(key, data);
		}

		public Boolean VerifyChild(Block child)
		{
			// simple check if signatures match
			if (Signature != child.ParentSignature)
				return false;
			
			// actual data integrity check that the signature of (parent signature + data) results in the child's signature
			var publicKey = NSec.Cryptography.PublicKey.Import(Algorithm, child.PublicKey.Value, PublicKeyBlobFormat);
			Span<Byte> data = stackalloc Byte[child.ParentSignature.Length + child.Data.Length];
			child.ParentSignature.Value.CopyTo(data);
			child.Data.Value.CopyTo(data.Slice(ParentSignature.Length));
			return Algorithm.Verify(publicKey, data, child.Signature.Value);
		}

		private static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;
		private static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;
	}
}
