using System;
using System.Collections.Generic;
using System.Text;

namespace BlockTree
{
	class TraversalExtensions
	{
		public static IEnumerable<T> TraverseDepthFirst<T>(T item, Func<T, IEnumerable<T>> childSelector)
		{
			var stack = new Stack<T>();
			stack.Push(item);
			while (stack.Count != 0)
			{
				var next = stack.Pop();
				yield return next;
				foreach (var child in childSelector(next))
					stack.Push(child);
			}
		}

		public static IEnumerable<T> TraverseBreadthFirst<T>(T item, Func<T, IEnumerable<T>> childSelector)
		{
			var stack = new Queue<T>();
			stack.Enqueue(item);
			while (stack.Count != 0)
			{
				var next = stack.Dequeue();
				yield return next;
				foreach (var child in childSelector(next))
					stack.Enqueue(child);
			}
		}
	}
}
