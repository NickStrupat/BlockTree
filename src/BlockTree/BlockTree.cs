using System;
using System.Collections.Generic;
using System.Linq;
using NSec.Cryptography;

namespace TheBlockTree
{
	// blocks are verified
	public class BlockTree
    {
		private readonly BlockIndex blockIndex;

		public Block Root { get; }

		public BlockTree(IEnumerable<Block> unverifiedBlocks)
		{
			var unverifiedBlockIndex = new BlockIndex();
			foreach (var unverifiedBlock in unverifiedBlocks)
				unverifiedBlockIndex.Add(unverifiedBlock);
			var rootBlock = unverifiedBlockIndex.GetChildren(ReadOnlyMemory<Byte>.Empty).SingleOrDefault();
			if (rootBlock == null)
				throw new NoRootBlock();
			VerifyChildrenDepthFirst(rootBlock);
			void VerifyChildrenDepthFirst(Block parent)
			{
				var children = unverifiedBlockIndex.GetChildren(parent);
				if (children == null)
					return;
				foreach (var child in children)
				{
					if (!parent.VerifyChild(child))
						throw new InvalidBlocksException(parent, child);
					VerifyChildrenDepthFirst(child);
				}
			}
			Root = rootBlock!;
			blockIndex = unverifiedBlockIndex;
		}
		
		public BlockTree(Byte[] rootData, Key key)
		{
			Root = new Block(Array.Empty<Byte>(), rootData, key);
			blockIndex = new BlockIndex();
			blockIndex.Add(Root);
		}

		public Boolean TryAdd(Block parent, Byte[] data, Key key, out Block? childBlock)
		{
			var maybe = blockIndex.GetBySignature(parent.Signature);
			if (maybe == null)
			{
				childBlock = null;
				return false;
			}
			childBlock = new Block(parent.Signature, data, key);
			blockIndex.Add(childBlock);
			return true;
		}

		public List<Block> GetAllBlocks() => blockIndex.GetAllBlocks();

		public class NoRootBlock : Exception {
			internal NoRootBlock() { }
		}

		public class InvalidBlocksException : Exception
		{
			public Block Parent { get; }
			public Block Child { get; }
			internal InvalidBlocksException(Block parent, Block child)
			{
				Parent = parent;
				Child = child;
			}
		}
	}
}