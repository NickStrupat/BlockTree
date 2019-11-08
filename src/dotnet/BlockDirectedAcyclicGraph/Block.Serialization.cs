using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NickStrupat
{
	public readonly partial struct Block
	{
		private Block(
			UInt32 version,
			SignatureAlgorithmCode signatureAlgorithmCode,
			ImmutableMemory<Byte> publicKey,
			ImmutableMemory<Byte> parentSignatures,
			ImmutableMemory<Byte> data,
			ImmutableMemory<Byte> nonce,
			ImmutableMemory<Byte> signature
		) : this()
		{
			Version = version;
			SignatureAlgorithmCode = signatureAlgorithmCode;
			ParentSignatures = parentSignatures;
			Data = data;
			Nonce = nonce;
			Signature = signature;
			PublicKey = publicKey;
		}

		private static readonly UInt32[] supportedVersions = new UInt32[] { 1 };

		private static Boolean DeserializeInternal(ImmutableMemory<Byte> rawBlockBytes, out Block block, [NotNullWhen(false)] out Exception? exception)
		{
			block = default;
			exception = default;

			var version = checked((UInt32)rawBlockBytes.ReadInt32AndAdvance());
			if (!supportedVersions.Contains(version))
			{
				exception = new InvalidVersionException(version);
				return false;
			}

			var signatureAlgorithmCode = (SignatureAlgorithmCode)rawBlockBytes.ReadInt32AndAdvance();
			if (!Enum.IsDefined(typeof(SignatureAlgorithmCode), signatureAlgorithmCode))
			{
				exception = new InvalidSignatureAlgorithmException(signatureAlgorithmCode);
				return false;
			}

			var publicKeyLength = rawBlockBytes.ReadInt32AndAdvance();
			if (publicKeyLength != Algorithm.PublicKeySize)
			{
				exception = new InvalidPublicKeyLengthException(publicKeyLength);
				return false;
			}
			var publicKey = rawBlockBytes.ReadBytesAndAdvance(publicKeyLength);

			var parentSignaturesLength = rawBlockBytes.ReadInt32AndAdvance();
			if (!IsParentSignaturesLengthValid(parentSignaturesLength))
			{
				exception = new InvalidParentSignaturesLengthException(parentSignaturesLength);
				return false;
			}
			var parentSignatures = rawBlockBytes.ReadBytesAndAdvance(parentSignaturesLength);

			var dataLength = rawBlockBytes.ReadInt32AndAdvance();
			var data = rawBlockBytes.ReadBytesAndAdvance(dataLength);

			var nonceLength = rawBlockBytes.ReadInt32AndAdvance();
			if (nonceLength != NonceByteLength)
			{
				exception = new InvalidNonceLengthException(parentSignaturesLength);
				return false;
			}
			var nonce = rawBlockBytes.ReadBytesAndAdvance(nonceLength);

			var signatureLength = rawBlockBytes.ReadInt32AndAdvance();
			if (signatureLength != Algorithm.SignatureSize)
			{
				exception = new InvalidSignatureLengthException(signatureLength);
				return false;
			}
			var signature = rawBlockBytes.ReadBytesAndAdvance(signatureLength);

			var unverifiedBlock = new Block(version, signatureAlgorithmCode, publicKey, parentSignatures, data, nonce, signature);
			if (!unverifiedBlock.VerifyParentSignaturesAndData())
			{
				exception = new BlockVerificationException(unverifiedBlock);
				return false;
			}

			block = unverifiedBlock;
			return true;
		}

		public static Boolean TryDeserialize(ImmutableMemory<Byte> rawBlockBytes, out Block block) =>
			DeserializeInternal(rawBlockBytes, out block, out _);

		public static Block Deserialize(ImmutableMemory<Byte> rawBlockBytes) =>
			DeserializeInternal(rawBlockBytes, out var block, out var exception) ? block : throw exception;

		public Int32 SerializationLength =>
			sizeof(Int32) +
			sizeof(Int32) +
			sizeof(Int32) + PublicKey.Length +
			sizeof(Int32) + ParentSignatures.Length +
			sizeof(Int32) + Data.Length +
			sizeof(Int32) + Nonce.Length +
			sizeof(Int32) + Signature.Length;

		public void Serialize(Span<Byte> destination)
		{
			destination.WriteInt32AndAdvance((Int32) Version);
			destination.WriteInt32AndAdvance((Int32) SignatureAlgorithmCode);

			destination.WriteInt32AndAdvance(PublicKey.Length);
			destination.WriteBytesAndAdvance(PublicKey);

			destination.WriteInt32AndAdvance(ParentSignatures.Length);
			destination.WriteBytesAndAdvance(ParentSignatures);

			destination.WriteInt32AndAdvance(Data.Length);
			destination.WriteBytesAndAdvance(Data);

			destination.WriteInt32AndAdvance(Nonce.Length);
			destination.WriteBytesAndAdvance(Nonce);

			destination.WriteInt32AndAdvance(Signature.Length);
			destination.WriteBytesAndAdvance(Signature);
		}
	}
}
