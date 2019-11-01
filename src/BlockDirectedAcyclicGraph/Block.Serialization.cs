using System;

namespace NickStrupat
{
	public readonly partial struct Block
	{
		private Block(
			ImmutableMemory<Byte> parentSignatures,
			ImmutableMemory<Byte> data,
			ImmutableMemory<Byte> signature,
			ImmutableMemory<Byte> publicKey
		) : this()
		{
			ParentSignatures = parentSignatures;
			Data = data;
			Signature = signature;
			PublicKey = publicKey;
		}

		public static Boolean TryDeserialize(ImmutableMemory<Byte> rawBlockBytes, out Block block)
		{
			var parentSignatures = rawBlockBytes.ReadAndAdvance();
			if (!IsParentSignaturesLengthValid(parentSignatures))
				throw new ArgumentException();
			var data = rawBlockBytes.ReadAndAdvance();
			var signature = rawBlockBytes.ReadAndAdvance();
			var publicKey = rawBlockBytes.ReadAndAdvance();

			var unverifiedBlock = new Block(parentSignatures, data, signature, publicKey);
			if (unverifiedBlock.VerifyParentSignaturesAndData())
			{
				block = unverifiedBlock;
				return true;
			}
			block = default;
			return false;
		}

		public static Block Deserialize(ImmutableMemory<Byte> rawBlockBytes)
		{
			if (!TryDeserialize(rawBlockBytes, out var block))
				throw new ArgumentException();
			return block;
		}

		public Int32 SerializationLength =>
			sizeof(Int32) + ParentSignatures.Length +
			sizeof(Int32) + Data.Length +
			sizeof(Int32) + Signature.Length +
			sizeof(Int32) + PublicKey.Length;

		public void Serialize(Span<Byte> destination)
		{
			destination.CopyFromAndAdvance(ParentSignatures);
			destination.CopyFromAndAdvance(Data);
			destination.CopyFromAndAdvance(Signature);
			destination.CopyFromAndAdvance(PublicKey);
		}
	}
}
