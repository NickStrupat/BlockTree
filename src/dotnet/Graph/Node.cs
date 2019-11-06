using System;
using System.Collections.Generic;

namespace NickStrupat.Graph
{
	public readonly struct Node
	{
		public ImmutableMemory<Node> Parents { get; }
		public ImmutableMemory<Byte> Data { get; }
		public IReadOnlyList<Node> Children => children;
		internal readonly List<Node> children;

		internal Node(ImmutableMemory<Node> parents, ImmutableMemory<Byte> data) => (Parents, Data, children) = (parents, data, new List<Node>());
	}

	public static class NodeExtensions
	{
		public static Node AddChild(this Node node, ImmutableMemory<Byte> data)
		{
			var parents = new ImmutableMemory<Node>(new Node[] { node });
			var child = new Node(parents, data);
			node.children.Add(child);
			return child;
		}
	}
}
