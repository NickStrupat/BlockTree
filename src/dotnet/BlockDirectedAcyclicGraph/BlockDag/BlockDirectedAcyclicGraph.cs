using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NickStrupat
{
	public sealed class BlockDirectedAcyclicGraph
	{
		private readonly BlockIndex blockIndex = new BlockIndex();

		public BlockDirectedAcyclicGraph(IEnumerable<Block> unverifiedBlocks)
		{
			// build up index of all blocks
			var unverifiedBlockIndex = new BlockIndex();
			foreach (var unverifiedBlock in unverifiedBlocks)
				unverifiedBlockIndex.Add(unverifiedBlock);

			// verify the blocks and build a tree, starting from each root
			if (!blockIndex.TryGetChildren(default, out var roots))
				throw new NoRootBlockException();

			foreach (var root in roots)
			{
				var queue = new Queue<Block>(unverifiedBlockIndex.Count / 2);
				queue.Enqueue(root);
				while (queue.Count != 0)
				{
					var current = queue.Dequeue();
					if (!unverifiedBlockIndex.TryGetChildren(current.Signature, out var children))
						continue;
					foreach (var child in children)
						if (IsParentOfChild(current, child))
							queue.Enqueue(child);
						else
							throw new InvalidBlocksException(current, child);
				}
			}

			blockIndex = unverifiedBlockIndex; // now verified
		}

		private static Boolean IsParentOfChild(Block parent, Block child) =>
			child.ParentSignatures.AsSpan().IndexOf(parent.Signature) != -1;

		public Boolean TryAdd(in Block parent, ImmutableMemory<Byte> data, Key key, [NotNullWhen(true)] out Block? child)
		{
			child = default;
			if (!blockIndex.TryGetBySignature(parent.Signature, out var foundParent))
				return false;
			if (!parent.Equals(foundParent))
				return false;
			var childBlock = new Block(parent, data, key);
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
			var childBlock = new Block(parent, data, key);
			blockIndex.Add(childBlock);
			return childBlock;
		}

		public IEnumerable<Block> GetAllBlocks() => blockIndex.GetAllBlocks();
	}
}
