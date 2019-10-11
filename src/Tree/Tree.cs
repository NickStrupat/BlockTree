#nullable enable
using System.Collections.Generic;

namespace Tree
{
	public sealed class Tree<T>
	{
		public TreeRootNode<T> Root { get; }

		public Tree(T rootValue) => Root = new TreeRootNode<T>(rootValue);

		public IEnumerable<TreeNodeBase<T>> TraverseDepthFirst()
		{
			yield return Root;
			foreach (var child in Root.TraverseDescendantsDepthFirst())
				yield return child;
		}

		public IEnumerable<TreeNodeBase<T>> TraverseBreadthFirst()
		{
			yield return Root;
			foreach (var child in Root.TraverseDescendantsBreadthFirst())
				yield return child;
		}
	}
}
