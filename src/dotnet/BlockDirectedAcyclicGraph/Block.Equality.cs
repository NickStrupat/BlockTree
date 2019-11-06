using System;
using System.Collections.Generic;

namespace NickStrupat
{
	public readonly partial struct Block : IEquatable<Block>
	{
		public override Boolean Equals(Object obj) => obj is Block block && Equals(block);

		public Boolean Equals(Block other) => EqualityComparer.Default.Equals(this, other);

		public override Int32 GetHashCode() => EqualityComparer.Default.GetHashCode(this);

		public sealed class EqualityComparer : IEqualityComparer<Block>
		{
			public static readonly EqualityComparer Default = new EqualityComparer();
			
			private EqualityComparer() { }

			public Boolean Equals(Block x, Block y) =>
				( // check lengths first because that's fast and rules out any obvious mismatches
					x.PublicKey.Length == y.PublicKey.Length &&
					x.ParentSignatures.Length == y.ParentSignatures.Length &&
					x.Data.Length == y.Data.Length &&
					x.Signature.Length == y.Signature.Length
				) &&
				(
					( // check if the spans are actually the exact same instance of span (e.g. comparing to itself)
						x.PublicKey.ImmutableSpan == y.PublicKey.ImmutableSpan &&
						x.ParentSignatures.ImmutableSpan == y.ParentSignatures.ImmutableSpan &&
						x.Data.ImmutableSpan == y.Data.ImmutableSpan &&
						x.Signature.ImmutableSpan == y.Signature.ImmutableSpan
					) ||
					( // actually compare the contents of the spans for equality
						x.PublicKey.ImmutableSpan.Span.SequenceEqual(y.PublicKey.ImmutableSpan.Span) &&
						x.ParentSignatures.ImmutableSpan.Span.SequenceEqual(y.ParentSignatures.ImmutableSpan.Span) &&
						x.Data.ImmutableSpan.Span.SequenceEqual(y.Data.ImmutableSpan.Span) &&
						x.Signature.ImmutableSpan.Span.SequenceEqual(y.Signature.ImmutableSpan.Span)
					)
				);

			public Int32 GetHashCode(Block block)
			{
				var hashCode = new HashCode();
				foreach (var @byte in block.Signature.ImmutableSpan)
					hashCode.Add(@byte);
				return hashCode.ToHashCode();
			}
		}
	}
}