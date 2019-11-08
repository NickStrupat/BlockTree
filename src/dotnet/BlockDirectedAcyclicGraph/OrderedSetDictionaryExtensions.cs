using Gma.DataStructures;
using System;
using System.Collections.Generic;

namespace NickStrupat
{
	static class OrderedSetDictionaryExtensions
	{
		public static Boolean TryAddValue<K, V>(this Dictionary<K, OrderedSet<V>> dictionary, K key, V value, IEqualityComparer<V>? equalityComparer = null)
		{
			if (dictionary.TryGetValue(key, out var values))
				return values.Add(value);
			dictionary.Add(key, new OrderedSet<V>(equalityComparer ?? EqualityComparer<V>.Default) { value });
			return true;
		}
	}
}