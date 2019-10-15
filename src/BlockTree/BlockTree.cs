using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NSec.Cryptography;
using Tree;

namespace TheBlockTree
{
	// blocks are verified
	public class BlockTree
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly BlockIndex blockIndex;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Tree<Block> tree;

		public TreeRootNode<Block> Root => tree.Root;
		public IEnumerable<(TreeNodeBase<Block> Node, UInt32 Level)> TraverseDepthFirst() => tree.TraverseDepthFirst();

		public BlockTree(IEnumerable<Block> unverifiedBlocks)
		{
			// build up index of all blocks
			var unverifieBlockIndex = new BlockIndex();
			foreach (var unverifiedBlock in unverifiedBlocks)
				unverifieBlockIndex.Add(unverifiedBlock);

			// get the root node
			var rootBlock = unverifieBlockIndex.TryGetChildren(ReadOnlyMemory<Byte>.Empty, out var foundBlocks) ?
				foundBlocks.Single() :
				throw new NoRootBlock();

			// verify the blocks and build a tree
			tree = new Tree<Block>(rootBlock);
			var queue = new Queue<TreeNodeBase<Block>>(unverifieBlockIndex.Count);
			queue.Enqueue(tree.Root);
			while (queue.Count != 0)
			{
				var current = queue.Dequeue();
				if (!unverifieBlockIndex.TryGetChildren(current.Value.Signature, out var children))
					continue;
				foreach (var child in children)
					if (current.Value.VerifyChild(child))
						queue.Enqueue(current.AddChildNode(child));
					else
						throw new InvalidBlocksException(current.Value, child);
			}

			blockIndex = unverifieBlockIndex; // now verified
		}

		
		public BlockTree(Byte[] rootData, Key key)
		{
			var rootBlock = new Block(ReadOnlyMemory<Byte>.Empty, rootData, key);
			blockIndex = new BlockIndex();
			blockIndex.Add(rootBlock);
			tree = new Tree<Block>(rootBlock);
		}

		public Boolean TryAdd(TreeNodeBase<Block> parent, Byte[] data, Key key, [NotNullWhen(true)] out TreeNodeBase<Block>? child)
		{
			if (!blockIndex.Contains(parent.Value.Signature))
			{
				child = null;
				return false;
			}
			var childBlock = new Block(parent.Value.Signature, data, key);
			blockIndex.Add(childBlock);
			child = parent.AddChildNode(childBlock);
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