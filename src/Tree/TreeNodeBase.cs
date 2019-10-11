#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

		public IEnumerable<TreeNodeBase<T>> TraverseDescendantsDepthFirst()
		{
			//var stack = new Stack<TreeNodeBase<T>>();
			//var node = this;
			//do
			//{
			//	foreach (var child in node.Children)
			//		stack.Push(child);
			//	var next = stack.Pop();
			//	yield return next;
			//}
			//while (stack.Count != 0);

			//stack.Push(this);
			//while (stack.Count != 0)
			//{
			//	var next = stack.Pop();
			//	yield return next;
			//	foreach (var child in next.Children)
			//		stack.Push(child);
			//}

			foreach (var child in Children)
			{
				yield return child;
				foreach (var grandChild in child.TraverseDescendantsDepthFirst())
					yield return grandChild;
			}
		}

		public IEnumerable<TreeNodeBase<T>> TraverseDescendantsBreadthFirst()
		{
			var queue = new Queue<TreeNodeBase<T>>();
			queue.Enqueue(this);
			while (queue.Count != 0)
			{
				var next = queue.Dequeue();
				yield return next;
				foreach (var child in next.Children)
					queue.Enqueue(child);
			}
			//foreach (var child in Children)
			//	yield return child;
			//foreach (var child in Children)
			//	foreach (var grandChild in child.TraverseDescendantsDepthFirst())
			//		yield return grandChild;
		}
	}
}