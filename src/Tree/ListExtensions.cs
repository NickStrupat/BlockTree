#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Tree
{
	// https://stackoverflow.com/q/12838122/232574
	public static class ListExtensions
	{
		public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			return list as IReadOnlyList<T> ?? new ReadOnlyListWrapper<T>(list);
		}

		private sealed class ReadOnlyListWrapper<T> : IReadOnlyList<T>
		{
			private readonly IList<T> list;

			public ReadOnlyListWrapper(IList<T> list) => this.list = list;

			public Int32 Count => list.Count;

			public T this[Int32 index] => list[index];

			public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}
}