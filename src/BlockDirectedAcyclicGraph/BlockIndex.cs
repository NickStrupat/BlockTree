using Gma.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NickStrupat
{
	// no verification of block signatures nor chaining is done by the index
	public sealed class BlockIndex
	{
		private readonly Dictionary<ImmutableMemory<Byte>, Block> blocksBySignature =
			new Dictionary<ImmutableMemory<Byte>, Block>(ImmutableMemoryEqualityComparer<Byte>.Instance);

		private readonly Dictionary<ImmutableMemory<Byte>, OrderedSet<Block>> childBlocksByParentSignature =
			new Dictionary<ImmutableMemory<Byte>, OrderedSet<Block>>(ImmutableMemoryEqualityComparer<Byte>.Instance);

		public Int32 Count => blocksBySignature.Count;

		public Boolean Contains(ImmutableMemory<Byte> signature) =>
			blocksBySignature.ContainsKey(signature);

		public Boolean TryGetBySignature(ImmutableMemory<Byte> signature, out Block block) =>
			blocksBySignature.TryGetValue(signature, out block);

		public Boolean TryGetChildren(ImmutableMemory<Byte> signature, [NotNullWhen(true)] out IReadOnlyList<Block>? children)
		{
			if (childBlocksByParentSignature.TryGetValue(signature, out var values))
			{
				children = values.ToList();
				return true;
			}
			children = null;
			return false;
		}

		public void Add(Block block)
		{
			try
			{
				blocksBySignature.Add(block.Signature, block);
				foreach (var parentSignature in block.ParentSignaturesEnumerable)
					if (!childBlocksByParentSignature.TryAddValue(parentSignature, block))
						throw new ArgumentException("An element with the same key already exists in the set");
			}
			catch
			{
				blocksBySignature.Remove(block.Signature);
				foreach (var parentSignature in block.ParentSignaturesEnumerable)
					if (childBlocksByParentSignature.TryGetValue(parentSignature, out var set))
						if (set.Remove(block) && set.Count == 0)
							childBlocksByParentSignature.Remove(parentSignature);
				throw;
			}
		}

		public IReadOnlyList<Block> GetAllBlocks() =>
			blocksBySignature.Values.ToList();

		public IEnumerable<(Block Block, UInt32 Level)> TraverseDepthFirst(Block root) => root.TraverseDepthFirst(
			x => TryGetChildren(x.Signature, out var children) ? children : Array.Empty<Block>()
		);

		public IEnumerable<(Block Block, UInt32 Level)> TraverseBreadthFirst(Block root) => root.TraverseBreadthFirst(
			x => TryGetChildren(x.Signature, out var children) ? children : Array.Empty<Block>()
		);
	}
}