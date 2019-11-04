using System;

namespace NickStrupat.Graph
{
	public class DirectedAcyclicGraph
	{
		public Node Root { get; }

		public DirectedAcyclicGraph() => Root = new Node(ImmutableMemory<Node>.Empty, ImmutableMemory<Byte>.Empty);
	}
}
