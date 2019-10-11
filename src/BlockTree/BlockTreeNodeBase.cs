#nullable enable
using System.Collections.Generic;

namespace TheBlockTree
{
	public abstract class BlockTreeNodeBase
	{
		public Block Block { get; }

		private readonly IList<BlockTreeNode> children = new List<BlockTreeNode>();
		public IReadOnlyList<BlockTreeNode> Children { get; }

		private protected BlockTreeNodeBase(Block block)
		{
			Block = block;
			Children = children.AsReadOnly();
		}

		public BlockTreeNode AddChildNode(Block block)
		{
			var node = new BlockTreeNode(this, block);
			children.Add(node);
			return node;
		}
	}
}