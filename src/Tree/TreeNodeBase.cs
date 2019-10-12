#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Tree
{
	public abstract class TreeNodeBase<T>
	{
		public T Value { get; }
		public IReadOnlyList<TreeNode<T>> Children { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly List<TreeNode<T>> children = new List<TreeNode<T>>();

		private protected TreeNodeBase(T value)
		{
			Value = value;
			Children = children.AsReadOnly();
		}

		public override String ToString() => Value?.ToString() ?? base.ToString();

		public TreeNode<T> AddChildNode(T value)
		{
			var node = new TreeNode<T>(this, value);
			children.Add(node);
			return node;
		}

		public void Print(TextWriter textWriter) => PrintInternal(textWriter, String.Empty, true);

		private void PrintInternal(TextWriter textWriter, String indent, Boolean last)
		{
			textWriter.Write(indent);
			if (last)
			{
				textWriter.Write("└─");
				indent += "  ";
			}
			else
			{
				textWriter.Write("├─");
				indent += "│ ";
			}
			textWriter.WriteLine(Value);

			for (var i = 0; i < Children.Count; i++)
				Children[i].PrintInternal(textWriter, indent, i == Children.Count - 1);
		}

		public IEnumerable<(TreeNodeBase<T> Value, UInt32 Level)> TraverseDescendantsDepthFirst()
		{
			var stack = new Stack<(TreeNodeBase<T> Value, UInt32 Level)>();
			foreach (var child in this.Children.Reverse())
				stack.Push((child, 0u));
			while (stack.Count != 0)
			{
				var next = stack.Pop();
				yield return next;
				foreach (var child in next.Value.Children.Reverse())
					stack.Push((child, next.Level + 1));
			}
		}

		public IEnumerable<(TreeNodeBase<T> Value, UInt32 Level)> TraverseDescendantsBreadthFirst()
		{
			var queue = new Queue<(TreeNodeBase<T> Value, UInt32 Level)>();
			foreach (var child in this.Children)
				queue.Enqueue((child, 0u));
			while (queue.Count != 0)
			{
				var next = queue.Dequeue();
				yield return next;
				foreach (var child in next.Value.Children)
					queue.Enqueue((child, next.Level + 1));
			}
		}
	}
}