using System;

namespace NickStrupat
{
	public abstract class BlockException : Exception
	{
		internal BlockException() {}
		internal BlockException(Exception innerException) : base(null, innerException) {}
	}

	public sealed class InvalidVersionException : BlockException
	{
		public UInt32 Version { get; }
		internal InvalidVersionException(UInt32 version) => Version = version;
	}

	public sealed class InvalidSignatureAlgorithmCodeException : BlockException
	{
		public SignatureAlgorithmCode SignatureAlgorithmCode { get; }
		internal InvalidSignatureAlgorithmCodeException(SignatureAlgorithmCode signatureAlgorithmCode) => SignatureAlgorithmCode = signatureAlgorithmCode;
	}

	public sealed class BlockVerificationException : BlockException
	{
		public Block UnverifiedBlock { get; }
		internal BlockVerificationException(Block unverifiedBlock) => UnverifiedBlock = unverifiedBlock;
	}

	public sealed class InvalidPublicKeyLengthException : BlockException
	{
		public Int32 ExpectedPublicKeyLength { get; }
		public Int32 ActualPublicKeyLength { get; }
		internal InvalidPublicKeyLengthException(Int32 expectedPublicKeyLength, Int32 actualPublicKeyLength) =>
			(ExpectedPublicKeyLength, ActualPublicKeyLength) = (expectedPublicKeyLength, actualPublicKeyLength);
	}

	public sealed class InvalidPublicKeyException : BlockException
	{
		internal InvalidPublicKeyException(Exception innerException) : base(innerException) { }
	}

	public sealed class InvalidParentSignaturesLengthException : BlockException
	{
		public Int32 ParentSignaturesLength { get; }
		internal InvalidParentSignaturesLengthException(Int32 parentSignaturesLength) => ParentSignaturesLength = parentSignaturesLength;
	}

	public sealed class InvalidSignatureLengthException : BlockException
	{
		public Int32 ExpectedSignatureLength { get; }
		public Int32 ActualSignatureLength { get; }
		internal InvalidSignatureLengthException(Int32 expectedSignatureLength, Int32 actualSignatureLength) =>
			(ExpectedSignatureLength, ActualSignatureLength) = (expectedSignatureLength, actualSignatureLength);
	}

	public sealed class InvalidNonceLengthException : BlockException
	{
		public Int32 NonceLength { get; }
		internal InvalidNonceLengthException(Int32 nonceLength) => NonceLength = nonceLength;
	}
}
