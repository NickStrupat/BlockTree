#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NSec.Cryptography;

namespace TheBlockTree
{
	static class ReadOnlySpanExtensions
	{
		// public static Boolean IsEqual<T>(this ReadOnlySpan<T> a, ReadOnlySpan<T> b) =>
		// 	a.Length == b.Length && (a == b || a.SequenceEqual<T>(b));
		public static Int32 SerializationSize(this ReadOnlySpan<Byte> bytes) => sizeof(Int32) + bytes.Length;
		public static void Serialize(this ReadOnlySpan<Byte> bytes, Span<Byte> destination)
		{
			MemoryMarshal.AsBytes(stackalloc Int32[] { bytes.Length }).CopyTo(destination);
			bytes.CopyTo(destination.Slice(sizeof(Int32)));
		}
	}
	[DebuggerTypeProxy(typeof(BlockDebugView))]
	public class Block : IEquatable<Block>
	{
		public readonly ReadOnlyMemory<Byte> ParentSignature;
		public readonly ReadOnlyMemory<Byte> Data;
		public readonly ReadOnlyMemory<Byte> Signature;
		public readonly ReadOnlyMemory<Byte> PublicKey;

		internal Block(ReadOnlyMemory<Byte> parentSignature, ReadOnlyMemory<Byte> data, Key key)
		{
			// make defensive copies since ReadOnlyMemory is just a view into memory that can be changed by whatever has the mutable memory
			ParentSignature = parentSignature.ToArray();
			Data = data.ToArray();

			// new arrays are made here so we don't need to make defensive copies
			PublicKey = key.PublicKey.Export(PublicKeyBlobFormat);
			Signature = SignParentSignatureAndData(key);
		}

		private Byte[] SignParentSignatureAndData(Key key)
		{
			Span<Byte> data = stackalloc Byte[ParentSignature.Length + Data.Length];
			ParentSignature.Span.CopyTo(data);
			Data.Span.CopyTo(data.Slice(ParentSignature.Length));
			return Algorithm.Sign(key, data);
		}

		public Boolean VerifyChild(Block child)
		{
			// simple check if signatures match
			if (!Signature.Span.SequenceEqual(child.ParentSignature.Span))
				return false;

			// actual data integrity check that the signature of (parent signature + data) results in the child's signature
			var publicKey = NSec.Cryptography.PublicKey.Import(Algorithm, child.PublicKey.Span, PublicKeyBlobFormat);
			Span<Byte> data = stackalloc Byte[child.ParentSignature.Length + child.Data.Length];
			child.ParentSignature.Span.CopyTo(data);
			child.Data.Span.CopyTo(data.Slice(child.ParentSignature.Length));
			return Algorithm.Verify(publicKey, data, child.Signature.Span);
		}

		public Byte[] ToBytes()
		{
			int parentSignatureSerializationSize = ParentSignature.Span.SerializationSize();
			int dataSerializationSize = Data.Span.SerializationSize();
			int signatureSerializationSize = Signature.Span.SerializationSize();
			int publicKeySerializationSize = PublicKey.Span.SerializationSize();

			var bytes = new Byte[
				parentSignatureSerializationSize +
				dataSerializationSize +
				signatureSerializationSize +
				publicKeySerializationSize
			];

			Span<Byte> destination = bytes;
			ParentSignature.Span.Serialize(destination);
			destination = destination.Slice(parentSignatureSerializationSize);
			ParentSignature.Span.Serialize(destination);
			destination = destination.Slice(dataSerializationSize);
			ParentSignature.Span.Serialize(destination);
			destination = destination.Slice(signatureSerializationSize);
			ParentSignature.Span.Serialize(destination);
			return bytes;
		}

		public override Int32 GetHashCode()
		{
			var hashCode = new HashCode();
			var bytes = ToBytes();
			foreach (var @byte in bytes)
				hashCode.Add(@byte);
			return hashCode.ToHashCode();
		}
		public override String ToString() => Convert.ToBase64String(ToBytes());
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
