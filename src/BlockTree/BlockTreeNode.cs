#nullable enable

namespace TheBlockTree
{
	public sealed class BlockTreeNode : BlockTreeNodeBase
	{
		public BlockTreeNodeBase Parent { get; }
		public BlockTreeNode(BlockTreeNodeBase parent, Block block) : base(block) => Parent = parent;
	}
}