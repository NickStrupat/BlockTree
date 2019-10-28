using System;

namespace BlockTree.Graph
{
	public class DirectedAcyclicGraph
	{
		public Node Root { get; }

		public DirectedAcyclicGraph() => Root = new Node(ReadOnlyMemory<Node>.Empty, ReadOnlyMemory<Byte>.Empty);
	}
}
