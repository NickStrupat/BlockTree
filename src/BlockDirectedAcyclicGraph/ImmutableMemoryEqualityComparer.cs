using System;
using System.Collections.Generic;

namespace NickStrupat
{
	public sealed class ImmutableMemoryEqualityComparer<T> : IEqualityComparer<ImmutableMemory<T>> where T : IEquatable<T>
	{
		public static readonly ImmutableMemoryEqualityComparer<T> Instance = new ImmutableMemoryEqualityComparer<T>();

		public bool Equals(ImmutableMemory<T> x, ImmutableMemory<T> y)
		{
			if (x.Equals(y)) // check if instances are the same
				return true;
			if (x.Length != y.Length)
				return false;
			return x.ImmutableSpan.Span.SequenceEqual(y.ImmutableSpan.Span);
		}

		public int GetHashCode(ImmutableMemory<T> obj)
		{
			var hashCode = new HashCode();
			foreach (var x in obj.ImmutableSpan)
				hashCode.Add(x);
			return hashCode.ToHashCode();
		}
	}
}
