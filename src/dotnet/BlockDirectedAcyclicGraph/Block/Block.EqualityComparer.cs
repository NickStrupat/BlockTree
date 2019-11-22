using System;
using System.Collections.Generic;

namespace NickStrupat
{
	public partial class Block
	{
		public sealed class EqualityComparer : IEqualityComparer<Block>
		{
			public static readonly EqualityComparer Default = new EqualityComparer();
			
			private EqualityComparer() { }

			public Boolean Equals(Block x, Block y) =>
				( // check scalars and lengths first because that's fast and rules out any obvious mismatches
					x.ParentSignatures.Length == y.ParentSignatures.Length &&
					x.Data.Length == y.Data.Length &&
					x.Signature.SignatureAlgorithmCode == y.Signature.SignatureAlgorithmCode &&
					x.Signature.Bytes.Length == y.Signature.Bytes.Length
				) &&
				(
					( // check if the members are actually the exact same instance (referential equality) (e.g. comparing to itself)
						x.ParentSignatures.AsSpan() == y.ParentSignatures.AsSpan() &&
						x.Data.AsSpan() == y.Data.AsSpan() &&
						x.Signature == y.Signature
					) ||
					( // actually compare the contents of the members for structural equality
						x.PublicKey.Equals(y.PublicKey) &&
						x.ParentSignatures.AsSpan().SequenceEqual(y.ParentSignatures.AsSpan()) &&
						x.Data.AsSpan().SequenceEqual(y.Data.AsSpan()) &&
						x.Signature.Bytes.AsSpan().SequenceEqual(y.Signature.Bytes.AsSpan())
					)
				);

			public Int32 GetHashCode(Block block) => block.Signature.GetHashCode();
		}
	}
}