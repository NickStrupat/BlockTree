using System;
using System.Diagnostics.CodeAnalysis;
using NSec.Cryptography;

namespace NickStrupat
{
	public static class SignatureAlgorithmCodeExtensions
	{
		public static Boolean TryGetSignatureAlgorithm(this SignatureAlgorithmCode signatureAlgorithmCode, [NotNullWhen(true)] out SignatureAlgorithm? signatureAlgorithm)
		{
			signatureAlgorithm = signatureAlgorithmCode.GetSignatureAlgorithmInternal();
			return signatureAlgorithm != null;
		}

		public static SignatureAlgorithm GetSignatureAlgorithm(this SignatureAlgorithmCode signatureAlgorithmCode) =>
			signatureAlgorithmCode.GetSignatureAlgorithmInternal() ?? throw new ArgumentOutOfRangeException(nameof(signatureAlgorithmCode));

		private static SignatureAlgorithm? GetSignatureAlgorithmInternal(this SignatureAlgorithmCode signatureAlgorithmCode) =>
			signatureAlgorithmCode switch
			{
				SignatureAlgorithmCode.Ed25519 => SignatureAlgorithm.Ed25519,
				_ => null
			};

		public static SignatureAlgorithmCode GetSignatureAlgorithm(this SignatureAlgorithm algorithm)
		{
			if (ReferenceEquals(algorithm, SignatureAlgorithm.Ed25519))
				return SignatureAlgorithmCode.Ed25519;
			else
				throw new ArgumentException("Algorithm not supported", nameof(algorithm));
		}

		public static Boolean Verify(this SignatureAlgorithmCode signatureAlgorithmCode, ReadOnlySpan<Byte> publicKeyBytes, ReadOnlySpan<Byte> data, ReadOnlySpan<Byte> signature)
		{
			switch (signatureAlgorithmCode)
			{
				case SignatureAlgorithmCode.Ed25519:
					var publicKey = NSec.Cryptography.PublicKey.Import(SignatureAlgorithm.Ed25519, publicKeyBytes, KeyBlobFormat.RawPublicKey);
					return SignatureAlgorithm.Ed25519.Verify(publicKey, data, signature);
				default:
					throw new ArgumentOutOfRangeException(nameof(signatureAlgorithmCode));
			}
		}
	}
}
