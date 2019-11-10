using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NickStrupat
{
	public sealed class BlockDirectedAcyclicGraph
	{
		public Block Root { get; }
		private readonly BlockIndex blockIndex = new BlockIndex();

		public BlockDirectedAcyclicGraph(IEnumerable<Block> unverifiedBlocks)
		{
			// build up index of all blocks
			var unverifiedBlockIndex = new BlockIndex();
			foreach (var unverifiedBlock in unverifiedBlocks)
				unverifiedBlockIndex.Add(unverifiedBlock);

			// get the root node
			Root = unverifiedBlockIndex.TryGetChildren(ImmutableMemory<Byte>.Empty, out var foundBlocks) ?
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
					if (Verify(current, child))
						queue.Enqueue(child);
					else
						throw new InvalidBlocksException(current, child);
			}

			blockIndex = unverifiedBlockIndex; // now verified
		}

		public BlockDirectedAcyclicGraph(ImmutableMemory<Byte> rootData, Key key)
		{
			Root = new Block(ImmutableMemory<Byte>.Empty, rootData, key);
			blockIndex = new BlockIndex();
			blockIndex.Add(Root);
		}

		private static Boolean Verify(Block parent, Block child)
		{
			if (!parent.Verify() || !child.Verify())
				return false;
			var pss = parent.Signature.AsSpan();
			foreach (var parentSignature in child.ParentSignaturesEnumerable)
			{
				var cpss = parentSignature.AsSpan();
				if (cpss == pss || cpss.SequenceEqual(pss))
					return true;
			}
			return false;
		}

		public Boolean TryAdd(in Block parent, ImmutableMemory<Byte> data, Key key, out Block child)
		{
			child = default;
			if (!blockIndex.TryGetBySignature(parent.Signature, out var foundParent))
				return false;
			if (!parent.Equals(foundParent))
				return false;
			var childBlock = new Block(parent.Signature, data, key);
			blockIndex.Add(childBlock);
			child = childBlock;
			return true;
		}

		public Block Add(in Block parent, ImmutableMemory<Byte> data, Key key)
		{
			if (!blockIndex.TryGetBySignature(parent.Signature, out var foundParent))
				throw new BlockNotFound(parent);
			if (!parent.Equals(foundParent))
				throw new SignatureCollisionException(parent, foundParent);
			var childBlock = new Block(parent.Signature, data, key);
			blockIndex.Add(childBlock);
			return childBlock;
		}

		public IEnumerable<Block> GetAllBlocks() => blockIndex.GetAllBlocks();
	}
}
