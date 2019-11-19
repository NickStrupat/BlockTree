using NSec.Cryptography;
using System;

namespace NickStrupat
{
	public sealed class InvalidAlgorithmException : Exception
	{
		public Algorithm Algorithm { get; }
		internal InvalidAlgorithmException(Algorithm algorithm) => Algorithm = algorithm;
	}
}
