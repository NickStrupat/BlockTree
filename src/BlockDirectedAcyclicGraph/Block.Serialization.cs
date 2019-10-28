using System;
using System.Buffers.Binary;

namespace BlockTree
{
	public readonly partial struct Block
	{
		public Block(ImmutableMemory<Byte> rawBlockBytes) : this()
		{
			var buffer = rawBlockBytes;
			var parentSignaturesLength = BinaryPrimitives.ReadInt32BigEndian(buffer.ImmutableSpan.Span);
			if (parentSignaturesLength == 0 || parentSignaturesLength % Algorithm.SignatureSize != 0)
				throw new ArgumentException();
			buffer = buffer.Slice(sizeof(Int32));

			parentSignaturesBytes = buffer.Slice(0, parentSignaturesLength);
			buffer = buffer.Slice(parentSignaturesLength);

			var dataLength = BinaryPrimitives.ReadInt32BigEndian(buffer.ImmutableSpan.Span);
			buffer = buffer.Slice(sizeof(Int32));

			Data = buffer.Slice(0, dataLength);
			buffer = buffer.Slice(dataLength);

			var signatureLength = BinaryPrimitives.ReadInt32BigEndian(buffer.ImmutableSpan.Span);
			if (signatureLength == 0 || signatureLength % Algorithm.SignatureSize != 0)
				throw new ArgumentException();
			buffer = buffer.Slice(sizeof(Int32));

			Signature = buffer.Slice(0, signatureLength);
			buffer = buffer.Slice(signatureLength);

			var publicKeyLength = BinaryPrimitives.ReadInt32BigEndian(buffer.ImmutableSpan.Span);
			if (publicKeyLength == 0 || publicKeyLength % Algorithm.PublicKeySize != 0)
				throw new ArgumentException();
			buffer = buffer.Slice(sizeof(Int32));

			PublicKey = buffer.Slice(0, publicKeyLength);

			if (!VerifyParentSignaturesAndData())
				throw new ArgumentException();
		}

		public Int32 SerializationLength =>
			parentSignaturesBytes.Length +
			sizeof(Int32) + Data.Length +
			sizeof(Int32) + Signature.Length +
			sizeof(Int32) + PublicKey.Length;

		public void SerializeTo(Span<Byte> destination)
		{
			var buffer = destination;
			BinaryPrimitives.WriteInt32BigEndian(buffer, parentSignaturesBytes.Length);
			buffer = buffer.Slice(sizeof(Int32));

			parentSignaturesBytes.ImmutableSpan.CopyTo(buffer);
			buffer = buffer.Slice(parentSignaturesBytes.Length);

			BinaryPrimitives.WriteInt32BigEndian(buffer, Data.Length);
			buffer = buffer.Slice(sizeof(Int32));

			Data.ImmutableSpan.CopyTo(buffer);
			buffer = buffer.Slice(Data.Length);

			BinaryPrimitives.WriteInt32BigEndian(buffer, Signature.Length);
			buffer = buffer.Slice(sizeof(Int32));

			Signature.ImmutableSpan.CopyTo(buffer);
			buffer = buffer.Slice(Signature.Length);

			BinaryPrimitives.WriteInt32BigEndian(buffer, PublicKey.Length);
			buffer = buffer.Slice(sizeof(Int32));

			PublicKey.ImmutableSpan.CopyTo(buffer);
			buffer = buffer.Slice(PublicKey.Length);
		}
	}
}
