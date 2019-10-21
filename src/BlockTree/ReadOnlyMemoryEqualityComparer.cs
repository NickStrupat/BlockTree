using System;
using System.Collections.Generic;

namespace TheBlockTree
{
	public sealed class ReadOnlyMemoryEqualityComparer<T> : IEqualityComparer<ReadOnlyMemory<T>> where T : IEquatable<T>
	{
		public static readonly ReadOnlyMemoryEqualityComparer<T> Instance = new ReadOnlyMemoryEqualityComparer<T>();

		public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y)
		{
			if (x.Equals(y)) // check if instances are the same
				return true;
			if (x.Length != y.Length)
				return false;
			return x.Span.SequenceEqual(y.Span);
		}

		public int GetHashCode(ReadOnlyMemory<T> obj)
		{
			var hashCode = new HashCode();
			foreach (var x in obj.Span)
				hashCode.Add(x);
			return hashCode.ToHashCode();
		}
	}
}
