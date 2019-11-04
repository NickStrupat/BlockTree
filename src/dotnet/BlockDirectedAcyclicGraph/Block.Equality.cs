using System;

namespace NickStrupat
{
	public readonly partial struct Block : IEquatable<Block>
	{
		public override Boolean Equals(Object obj) => obj is Block block && Equals(block);

		public Boolean Equals(Block other) =>
			( // check lengths first because that's fast and rules out any obvious mismatches
				PublicKey.Length == other.PublicKey.Length &&
				ParentSignatures.Length == other.ParentSignatures.Length &&
				Data.Length == other.Data.Length &&
				Signature.Length == other.Signature.Length
			) &&
			(
				( // check if the spans are actually the exact same instance of span (e.g. comparing to itself)
					PublicKey.ImmutableSpan == other.PublicKey.ImmutableSpan &&
					ParentSignatures.ImmutableSpan == other.ParentSignatures.ImmutableSpan &&
					Data.ImmutableSpan == other.Data.ImmutableSpan &&
					Signature.ImmutableSpan == other.Signature.ImmutableSpan
				) ||
				( // actually compare the contents of the spans for equality
					PublicKey.ImmutableSpan.Span.SequenceEqual(other.PublicKey.ImmutableSpan.Span) &&
					ParentSignatures.ImmutableSpan.Span.SequenceEqual(other.ParentSignatures.ImmutableSpan.Span) &&
					Data.ImmutableSpan.Span.SequenceEqual(other.Data.ImmutableSpan.Span) &&
					Signature.ImmutableSpan.Span.SequenceEqual(other.Signature.ImmutableSpan.Span)
				)
			);

		public override Int32 GetHashCode()
		{
			var hashCode = new HashCode();
			foreach (var @byte in Signature.ImmutableSpan)
				hashCode.Add(@byte);
			return hashCode.ToHashCode();
		}
	}
}