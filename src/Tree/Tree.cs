#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tree
{
	public sealed class Tree<T>
	{
		public TreeRootNode<T> Root { get; }

		public Tree(T rootValue) => Root = new TreeRootNode<T>(rootValue);

		public IEnumerable<(TreeNodeBase<T> Value, UInt32 Level)> TraverseDepthFirst()
		{
			yield return (Root, 0u);
			foreach (var child in Root.TraverseDescendantsDepthFirst())
				yield return (child.Value, child.Level + 1);
		}

		public IEnumerable<(TreeNodeBase<T> Value, UInt32 Level)> TraverseBreadthFirst()
		{
			yield return (Root, 0u);
			foreach (var child in Root.TraverseDescendantsBreadthFirst())
				yield return (child.Value, child.Level + 1);
		}
	}
}
