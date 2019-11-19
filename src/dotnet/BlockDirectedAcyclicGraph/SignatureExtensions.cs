using System;

namespace NickStrupat
{
	public static class SignatureExtensions
	{
		public static Int32 GetSerializationLength(this ReadOnlySpan<Signature> signatures)
		{
			var i = sizeof(Int32);
			foreach (var signature in signatures)
				i += signature.SerializationLength;
			return i;
		}

		public static void SerializeTo(this ReadOnlySpan<Signature> signatures, Span<Byte> destination) =>
			signatures.SerializeToAndAdvance(ref destination);

		public static void SerializeToAndAdvance(this ReadOnlySpan<Signature> signatures, ref Span<Byte> destination)
		{
			destination.WriteInt32AndAdvance(signatures.Length);
			foreach (var signature in signatures)
				signature.SerializeToAndAdvance(ref destination);
		}
	}
}
