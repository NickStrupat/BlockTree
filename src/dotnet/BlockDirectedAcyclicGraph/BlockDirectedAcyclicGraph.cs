using System;

namespace NickStrupat
{
	public readonly struct BlockDirectedAcyclicGraph
	{
		public Block Root { get; }
		public BlockDirectedAcyclicGraph(Block root) => Root = root;
	}
}
