using System;

namespace BlockTree
{
	public readonly struct BlockDirectedAcyclicGraph
	{
		public Block Root { get; }
		public BlockDirectedAcyclicGraph() => Root = new Block()
	}
}
