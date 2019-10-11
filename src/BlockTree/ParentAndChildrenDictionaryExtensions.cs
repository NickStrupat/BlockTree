using System.Collections.Generic;

namespace TheBlockTree
{
	static class ParentAndChildrenDictionaryExtensions
	{
		public static void AddAsParentAndChild<K, V>(this Dictionary<K, (V Parent, List<V> Children)> dictionary, K key, V valueToAdd)
		{
			if (dictionary.TryGetValue(key, out var values))
				values.Children.Add(valueToAdd);
			else
				dictionary.Add(key, new List<V> { valueToAdd });
		}
	}
}