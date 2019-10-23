using Gma.DataStructures;
using System.Collections.Generic;

namespace TheBlockTree
{
	static class MultiValueDictionaryExtensions
	{
		public static void AddValue<K, V>(this Dictionary<K, OrderedSet<V>> dictionary, K key, V value)
		{
			if (dictionary.TryGetValue(key, out var values))
				values.Add(value);
			else
				dictionary.Add(key, new OrderedSet<V> { value });
		}
	}
}