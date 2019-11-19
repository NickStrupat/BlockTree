using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NSec.Cryptography;

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
			var publicKeyLength = rawBlockBytes.ReadInt32AndAdvance();
			if (publicKeyLength != SignatureAlgorithm.PublicKeySize)
			{
				exception = new InvalidPublicKeyLengthException(publicKeyLength);
				return false;
			}
			var publicKeyBytes = rawBlockBytes.ReadBytesAndAdvance(publicKeyLength);
			if (!PublicKey.TryImport(SignatureAlgorithm, publicKeyBytes.AsSpan(), PublicKeyBlobFormat, out var publicKey))
			{
				// get the exception for the failing public key import
				try
				{
					PublicKey.Import(SignatureAlgorithm, publicKeyBytes.AsSpan(), PublicKeyBlobFormat);
				}
				catch (Exception ex)
				{
					exception = new InvalidPublicKeyException(ex);
				}
				return false;
			}

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
			sizeof(Int32) + PublicKey.Size +
			ParentSignatures.AsSpan().GetSerializationLength() +
			sizeof(Int32) + Data.Length +
			Signature.SerializationLength;

		public void Serialize(Span<Byte> destination)
		{
			var publicKeyBytes = PublicKey.Export(PublicKeyBlobFormat);
			destination.WriteInt32AndAdvance(publicKeyBytes.Length);
			destination.WriteBytesAndAdvance(publicKeyBytes.AsSpan());

			ParentSignatures.AsSpan().SerializeToAndAdvance(ref destination);

			destination.WriteInt32AndAdvance(Data.Length);
			destination.WriteBytesAndAdvance(Data);

			Signature.SerializeTo(destination);
		}
	}
}
