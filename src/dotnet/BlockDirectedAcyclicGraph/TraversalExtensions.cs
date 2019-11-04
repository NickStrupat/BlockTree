using System;
using System.Collections.Generic;

namespace NickStrupat
{
	public static class TraversalExtensions
	{
		public static IEnumerable<(T Node, UInt32 Level)> TraverseDepthFirst<T>(this T root, Func<T, IEnumerable<T>> childSelector)
		{
			var stack = new Stack<(T Node, UInt32 Level)>();
			stack.Push((root, 0u));
			while (stack.Count != 0)
			{
				var next = stack.Pop();
				yield return next;
				foreach (var child in childSelector(next.Node))
					stack.Push((child, next.Level + 1));
			}
		}

		public static IEnumerable<(T Node, UInt32 Level)> TraverseBreadthFirst<T>(this T root, Func<T, IEnumerable<T>> childSelector)
		{
			var stack = new Queue<(T Node, UInt32 Level)>();
			stack.Enqueue((root, 0u));
			while (stack.Count != 0)
			{
				var next = stack.Dequeue();
				yield return next;
				foreach (var child in childSelector(next.Node))
					stack.Enqueue((child, next.Level + 1));
			}
		}
	}
}
