using System;

namespace NickStrupat
{
	public sealed class NoRootBlockException : Exception
	{
		internal NoRootBlockException() { }
	}

	public sealed class InvalidBlocksException : Exception
	{
		public Block Parent { get; }
		public Block Child { get; }
		internal InvalidBlocksException(Block parent, Block child)
		{
			Parent = parent;
			Child = child;
		}
	}

	public sealed class BlockNotFound : Exception
	{
		public Block Block { get; }
		internal BlockNotFound(Block block) => Block = block;
	}

	public sealed class SignatureCollisionException : Exception
	{
		public Block Block1 { get; }
		public Block Block2 { get; }
		internal SignatureCollisionException(Block block1, Block block2)
		{
			Block1 = block1;
			Block2 = block2;
		}
	}
}
