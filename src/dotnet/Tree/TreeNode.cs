namespace Tree
{
	public sealed class TreeNode<T> : TreeNodeBase<T>
	{
		public TreeNodeBase<T> Parent { get; }
		internal TreeNode(TreeNodeBase<T> parent, T value) : base(value) => Parent = parent;
	}
}