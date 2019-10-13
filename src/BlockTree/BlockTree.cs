using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

			var rootBlock = unverifiedBlockIndex.TryGetChildren(ReadOnlyMemory<Byte>.Empty, out var foundBlocks) ?
				foundBlocks.Single() : 
				throw new NoRootBlock();

			VerifyChildrenDepthFirst(rootBlock);
			void VerifyChildrenDepthFirst(Block parent)
			{
				if (!unverifiedBlockIndex.TryGetChildren(parent.Signature, out var children))
					return;
				foreach (var child in children)
				{
					if (!parent.VerifyChild(child))
						throw new InvalidBlocksException(parent, child);
					VerifyChildrenDepthFirst(child);
				}
			}
			Root = rootBlock;
			blockIndex = unverifiedBlockIndex;
		}
		
		public BlockTree(Byte[] rootData, Key key)
		{
			Root = new Block(ReadOnlyMemory<Byte>.Empty, rootData, key);
			blockIndex = new BlockIndex();
			blockIndex.Add(Root);
		}

		public Boolean TryAdd(Block parent, Byte[] data, Key key, [NotNullWhen(true)] out Block? childBlock)
		{
			if (!blockIndex.Contains(parent.Signature))
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