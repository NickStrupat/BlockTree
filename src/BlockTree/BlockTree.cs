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
	public sealed class BlockTree
	{
		public Block Root { get; }
		private readonly BlockIndex blockIndex;

		public IEnumerable<(TreeNodeBase<Block> Node, UInt32 Level)> TraverseDepthFirst() => tree.TraverseDepthFirst();
		public IEnumerable<(TreeNodeBase<Block> Node, UInt32 Level)> TraverseBreadthFirst() => tree.TraverseBreadthFirst();

		public BlockTree(IEnumerable<Block> unverifiedBlocks)
		{
			// build up index of all blocks
			var unverifiedBlockIndex = new BlockIndex();
			foreach (var unverifiedBlock in unverifiedBlocks)
				unverifiedBlockIndex.Add(unverifiedBlock);

			// get the root node
			Root = unverifiedBlockIndex.TryGetChildren(ReadOnlyMemory<Byte>.Empty, out var foundBlocks) ?
				foundBlocks.Single() :
				throw new NoRootBlockException();

			// verify the blocks and build a tree
			var queue = new Queue<Block>(unverifiedBlockIndex.Count / 2);
			queue.Enqueue(Root);
			while (queue.Count != 0)
			{
				var current = queue.Dequeue();
				if (!unverifiedBlockIndex.TryGetChildren(current.Signature, out var children))
					continue;
				foreach (var child in children)
					if (current.VerifyChild(child))
						queue.Enqueue(child);
					else
						throw new InvalidBlocksException(current, child);
			}

			blockIndex = unverifiedBlockIndex; // now verified
		}
		
		public BlockTree(Byte[] rootData, Key key)
		{
			Root = new Block(ReadOnlySpan<Byte>.Empty, rootData, key);
			blockIndex = new BlockIndex();
			blockIndex.Add(Root);
		}

		public Boolean TryAdd(Block parent, Byte[] data, Key key, [NotNullWhen(true)] out Block? child)
		{
			blockIndex.TryGetBySignature(parent.Signature, out var existingBlock);
			if (!blockIndex.Contains(parent.Signature))
			{
				child = null;
				return false;
			}
			var childBlock = new Block(parent.Signature.Span, data, key);
			blockIndex.Add(childBlock);
			child = parent.AddChildNode(childBlock);
			return true;
		}

		public IReadOnlyList<Block> GetAllBlocks() => blockIndex.GetAllBlocks();

		public class NoRootBlockException : Exception {
			internal NoRootBlockException() { }
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