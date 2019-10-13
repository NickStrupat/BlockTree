using System.Collections.Generic;
using System.Diagnostics;

namespace TheBlockTree
{
	public abstract class BlockTreeNodeBase
	{
		public Block Block { get; }
		public IReadOnlyList<BlockTreeNode> Children { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<BlockTreeNode> children = new List<BlockTreeNode>();

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