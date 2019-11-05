using System;

namespace NickStrupat
{
	public abstract class BlockException : Exception
	{
		internal BlockException() {}
	}

	public class BlockVerificationException : BlockException
	{
		internal BlockVerificationException() {}
	}

	public class InvalidPublicKeyLengthException : BlockException
	{
		internal InvalidPublicKeyLengthException() {}
	}

	public class InvalidParentSignaturesLengthException : BlockException
	{
		internal InvalidParentSignaturesLengthException() {}
	}

	public class InvalidSignatureLengthException : BlockException
	{
		internal InvalidSignatureLengthException() {}
	}
}
