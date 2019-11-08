using System;

namespace NickStrupat
{
	public abstract class BlockException : Exception
	{
		internal BlockException() {}
	}

	public sealed class InvalidVersionException : BlockException
	{
		public UInt32 Version { get; }
		internal InvalidVersionException(UInt32 version) => Version = version;
	}

	public sealed class InvalidSignatureAlgorithmException : BlockException
	{
		public SignatureAlgorithmCode SignatureAlgorithmCode { get; }
		internal InvalidSignatureAlgorithmException(SignatureAlgorithmCode signatureAlgorithmCode) => SignatureAlgorithmCode = signatureAlgorithmCode;
	}

	public sealed class BlockVerificationException : BlockException
	{
		public Block UnverifiedBlock { get; }
		internal BlockVerificationException(Block unverifiedBlock) => UnverifiedBlock = unverifiedBlock;
	}

	public sealed class InvalidPublicKeyLengthException : BlockException
	{
		public Int32 PublicKeyLength { get; }
		internal InvalidPublicKeyLengthException(Int32 publicKeyLength) => PublicKeyLength = publicKeyLength;
	}

	public sealed class InvalidParentSignaturesLengthException : BlockException
	{
		public Int32 ParentSignaturesLength { get; }
		internal InvalidParentSignaturesLengthException(Int32 parentSignaturesLength) => ParentSignaturesLength = parentSignaturesLength;
	}

	public sealed class InvalidSignatureLengthException : BlockException
	{
		public Int32 SignatureLength { get; }
		internal InvalidSignatureLengthException(Int32 signatureLength) => SignatureLength = signatureLength;
	}
}
