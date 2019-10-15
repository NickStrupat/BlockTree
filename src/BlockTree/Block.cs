using System;
using System.Diagnostics;
using NSec.Cryptography;

namespace TheBlockTree
{
	[DebuggerTypeProxy(typeof(BlockDebugView))]
	public class Block : IEquatable<Block>
	{
		public readonly ReadOnlyMemory<Byte> ParentSignature;
		public readonly ReadOnlyMemory<Byte> Data;
		public readonly ReadOnlyMemory<Byte> Signature;
		public readonly ReadOnlyMemory<Byte> PublicKey;

		internal Block(ReadOnlySpan<Byte> parentSignature, ReadOnlySpan<Byte> data, Key key)
		{
			// make defensive copies since ReadOnlyMemory is just a view into memory that can be changed by whatever has the mutable memory
			ParentSignature = parentSignature.ToArray();
			Data = data.ToArray();

			// new arrays are made here so we don't need to make defensive copies
			PublicKey = key.PublicKey.Export(PublicKeyBlobFormat);
			Signature = SignParentSignatureAndData(key);
		}

		private Int32 BytesForCryptoLength => ParentSignature.Length + Data.Length;
		private void CopyBytesForCryptoTo(Span<Byte> destination)
		{
			ParentSignature.Span.CopyTo(destination);
			Data.Span.CopyTo(destination.Slice(ParentSignature.Length));
		}

		private Byte[] SignParentSignatureAndData(Key key)
		{
			Span<byte> bytesForCrypto = stackalloc Byte[BytesForCryptoLength];
			CopyBytesForCryptoTo(bytesForCrypto);
			return Algorithm.Sign(key, bytesForCrypto);
		}

		private Boolean VerifyParentSignatureAndData()
		{
			Span<byte> bytesForCrypto = stackalloc Byte[BytesForCryptoLength];
			CopyBytesForCryptoTo(bytesForCrypto);
			var publicKey = NSec.Cryptography.PublicKey.Import(Algorithm, PublicKey.Span, PublicKeyBlobFormat);
			return Algorithm.Verify(publicKey, bytesForCrypto, Signature.Span);
		}

		public Boolean VerifyChild(Block child)
		{
			// simple check if signatures match
			if (!Signature.Span.SequenceEqual(child.ParentSignature.Span))
				return false;

			// actual data integrity check that the signature of (parent signature + data) results in the child's signature
			return child.VerifyParentSignatureAndData();
		}

		public Int32 BytesLength =>
			ParentSignature.Span.GetSerializationLength() +
			Data.Span.GetSerializationLength() +
			Signature.Span.GetSerializationLength() +
			PublicKey.Span.GetSerializationLength();


		public void CopyBytesTo(Span<Byte> destination)
		{
			var buffer = destination;
			ParentSignature.Span.Serialize(buffer);
			buffer = buffer.Slice(ParentSignature.Span.GetSerializationLength());
			Data.Span.Serialize(buffer);
			buffer = buffer.Slice(Data.Span.GetSerializationLength());
			Signature.Span.Serialize(buffer);
			buffer = buffer.Slice(Signature.Span.GetSerializationLength());
			PublicKey.Span.Serialize(buffer);
		}

		public override Int32 GetHashCode()
		{
			var hashCode = new HashCode();
			Span<Byte> bytes = stackalloc Byte[BytesLength];
			CopyBytesTo(bytes);
			foreach (var @byte in bytes)
				hashCode.Add(@byte);
			return hashCode.ToHashCode();
		}
		public override String ToString()
		{
			Span<Byte> bytes = stackalloc Byte[BytesLength];
			CopyBytesTo(bytes);
			return Convert.ToBase64String(bytes);
		}

		public override Boolean Equals(Object o) => o is Block && this.Equals(o);
		public Boolean Equals(Block other) =>
			(
				ParentSignature.Length == other.ParentSignature.Length &&
				Data.Length == other.Data.Length &&
				Signature.Length == other.Signature.Length &&
				PublicKey.Length == other.PublicKey.Length
			) &&
			(
				(
					ParentSignature.Span == other.ParentSignature.Span &&
					Data.Span == other.Data.Span &&
					Signature.Span == other.Signature.Span &&
					PublicKey.Span == other.PublicKey.Span
				) ||
				(
					ParentSignature.Span.SequenceEqual(other.ParentSignature.Span) &&
					Data.Span.SequenceEqual(other.Data.Span) &&
					Signature.Span.SequenceEqual(other.Signature.Span) &&
					PublicKey.Span.SequenceEqual(other.PublicKey.Span)
				)
			);

		private static readonly SignatureAlgorithm Algorithm = SignatureAlgorithm.Ed25519;
		private static readonly KeyBlobFormat PublicKeyBlobFormat = KeyBlobFormat.RawPublicKey;

		internal class BlockDebugView
		{
			public String ParentSignature { get; }
			public String Data { get; }
			public String Signature { get; }
			public String PublicKey { get; }

			public BlockDebugView(Block block)
			{
				ParentSignature = Convert.ToBase64String(block.ParentSignature.Span);
				Data = Convert.ToBase64String(block.Data.Span);
				Signature = Convert.ToBase64String(block.Signature.Span);
				PublicKey = Convert.ToBase64String(block.PublicKey.Span);
			}
		}
	}
}
