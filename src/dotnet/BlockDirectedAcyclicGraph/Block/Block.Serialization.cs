using System;
using System.Diagnostics.CodeAnalysis;

namespace NickStrupat
{
	public partial class Block
	{
		private Block(
			PublicKey publicKey,
			ImmutableMemory<Signature> parentSignatures,
			ImmutableMemory<Byte> data,
			Signature signature
		)
		{
			PublicKey = publicKey;
			ParentSignatures = parentSignatures;
			Data = data;
			Signature = signature;
		}

		private static Boolean DeserializeInternal(ImmutableMemory<Byte> rawBlockBytes, [NotNullWhen(true)] out Block? block, [NotNullWhen(false)] out Exception? exception)
		{
			block = default;
			exception = default;

			// read public key
			var publicKey = PublicKey.DeserializeFromAndAdvance(ref rawBlockBytes);

			// read parent signatures
			var parentSignaturesCount = rawBlockBytes.ReadInt32AndAdvance();
			var parentSignaturesBytesConsumedCount = 0;
			var parentSignatures = ImmutableMemory<Signature>.Create(parentSignaturesCount, rawBlockBytes, (span, buffer) =>
			{
				for (var i = 0; i != span.Length; i++)
				{
					var signature = Signature.DeserializeFrom(buffer);
					parentSignaturesBytesConsumedCount += signature.SerializationLength;
					buffer = buffer.Slice(signature.SerializationLength);
				}
			});
			rawBlockBytes = rawBlockBytes.Slice(parentSignaturesBytesConsumedCount);

			// read data
			var dataLength = rawBlockBytes.ReadInt32AndAdvance();
			var data = rawBlockBytes.ReadBytesAndAdvance(dataLength);

			// read signature
			var signature = Signature.DeserializeFrom(rawBlockBytes);

			// create `Block` instance
			var unverifiedBlock = new Block(publicKey, parentSignatures, data, signature);
			if (!unverifiedBlock.VerifyParentSignaturesAndData())
			{
				exception = new BlockVerificationException(unverifiedBlock);
				return false;
			}

			block = unverifiedBlock;
			return true;
		}

		public static Boolean TryDeserialize(ImmutableMemory<Byte> rawBlockBytes, [NotNullWhen(true)] out Block? block) =>
			DeserializeInternal(rawBlockBytes, out block, out _);

		public static Block Deserialize(ImmutableMemory<Byte> rawBlockBytes) =>
			DeserializeInternal(rawBlockBytes, out var block, out var exception) ? block : throw exception;

		public Int32 SerializationLength =>
			PublicKey.SerializationLength +
			ParentSignatures.AsSpan().GetSerializationLength() +
			sizeof(Int32) + Data.Length +
			Signature.SerializationLength;

		public void Serialize(Span<Byte> destination)
		{
			PublicKey.SerializeToAndAdvance(ref destination);

			ParentSignatures.AsSpan().SerializeToAndAdvance(ref destination);

			destination.WriteInt32AndAdvance(Data.Length);
			destination.WriteBytesAndAdvance(Data);

			Signature.SerializeTo(destination);
		}
	}
}
