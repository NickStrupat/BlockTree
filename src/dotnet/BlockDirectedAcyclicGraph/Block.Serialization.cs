using System;

namespace NickStrupat
{
	public readonly partial struct Block
	{
		private Block(
			ImmutableMemory<Byte> publicKey,
			ImmutableMemory<Byte> parentSignatures,
			ImmutableMemory<Byte> data,
			ImmutableMemory<Byte> signature
		) : this()
		{
			ParentSignatures = parentSignatures;
			Data = data;
			Signature = signature;
			PublicKey = publicKey;
			IsVerified = VerifyParentSignaturesAndData();
		}

		public static Boolean TryDeserialize(ImmutableMemory<Byte> rawBlockBytes, out Block block)
		{
			block = default;

			var publicKeyLength = rawBlockBytes.ReadInt32AndAdvance();
			if (publicKeyLength != Algorithm.PublicKeySize)
				return false;
			var publicKey = rawBlockBytes.ReadBytesAndAdvance(publicKeyLength);

			var parentSignaturesLength = rawBlockBytes.ReadInt32AndAdvance();
			if (!IsParentSignaturesLengthValid(parentSignaturesLength))
				return false;
			var parentSignatures = rawBlockBytes.ReadBytesAndAdvance(parentSignaturesLength);

			var dataLength = rawBlockBytes.ReadInt32AndAdvance();
			var data = rawBlockBytes.ReadBytesAndAdvance(dataLength);

			var signatureLength = rawBlockBytes.ReadInt32AndAdvance();
			if (signatureLength != Algorithm.SignatureSize)
				return false;
			var signature = rawBlockBytes.ReadBytesAndAdvance(signatureLength);

			var unverifiedBlock = new Block(publicKey, parentSignatures, data, signature);
			if (!unverifiedBlock.IsVerified)
				return false;

			block = unverifiedBlock;
			return true;
		}

		public static Block Deserialize(ImmutableMemory<Byte> rawBlockBytes)
		{
			var publicKeyLength = rawBlockBytes.ReadInt32AndAdvance();
			if (publicKeyLength != Algorithm.PublicKeySize)
				throw new InvalidPublicKeyLengthException();
			var publicKey = rawBlockBytes.ReadBytesAndAdvance(publicKeyLength);

			var parentSignaturesLength = rawBlockBytes.ReadInt32AndAdvance();
			if (!IsParentSignaturesLengthValid(parentSignaturesLength))
				throw new InvalidParentSignaturesLengthException();
			var parentSignatures = rawBlockBytes.ReadBytesAndAdvance(parentSignaturesLength);

			var dataLength = rawBlockBytes.ReadInt32AndAdvance();
			var data = rawBlockBytes.ReadBytesAndAdvance(dataLength);

			var signatureLength = rawBlockBytes.ReadInt32AndAdvance();
			if (signatureLength != Algorithm.SignatureSize)
				throw new InvalidSignatureLengthException();
			var signature = rawBlockBytes.ReadBytesAndAdvance(signatureLength);

			var unverifiedBlock = new Block(publicKey, parentSignatures, data, signature);
			if (!unverifiedBlock.IsVerified)
				throw new BlockVerificationException();

			return unverifiedBlock;
		}

		public Int32 SerializationLength =>
			sizeof(Int32) + PublicKey.Length +
			sizeof(Int32) + ParentSignatures.Length +
			sizeof(Int32) + Data.Length +
			sizeof(Int32) + Signature.Length;

		public void Serialize(Span<Byte> destination)
		{
			destination.WriteInt32AndAdvance(PublicKey.Length);
			destination.WriteBytesAndAdvance(PublicKey);

			destination.WriteInt32AndAdvance(ParentSignatures.Length);
			destination.WriteBytesAndAdvance(ParentSignatures);

			destination.WriteInt32AndAdvance(Data.Length);
			destination.WriteBytesAndAdvance(Data);

			destination.WriteInt32AndAdvance(Signature.Length);
			destination.WriteBytesAndAdvance(Signature);
		}
	}
}
