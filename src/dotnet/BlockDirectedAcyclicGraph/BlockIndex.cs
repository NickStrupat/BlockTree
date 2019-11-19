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
		private readonly Dictionary<Signature, Block> blocksBySignature = new Dictionary<Signature, Block>();
		private readonly Dictionary<Signature, OrderedSet<Block>> childBlocksByParentSignature = new Dictionary<Signature, OrderedSet<Block>>();

		public Int32 Count => blocksBySignature.Count;

		public Boolean Contains(Signature signature) =>
			blocksBySignature.ContainsKey(signature);

		public Boolean TryGetBySignature(Signature signature, out Block block) =>
			blocksBySignature.TryGetValue(signature, out block);

		public Boolean TryGetChildren(Signature signature, [NotNullWhen(true)] out IReadOnlyList<Block>? children)
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

				if (block.ParentSignatures.IsEmpty)
					AddOrThrow(default, block);
				else
					foreach (var parentSignature in block.ParentSignatures.AsImmutableSpan())
						AddOrThrow(parentSignature, block);

				void AddOrThrow(Signature parentSignature, Block block)
				{
					if (!childBlocksByParentSignature.TryAddValue(parentSignature, block, Block.EqualityComparer.Default))
						throw new ArgumentException("An element with the same key already exists in the set");
				}
			}
			catch
			{
				blocksBySignature.Remove(block.Signature);
				if (block.ParentSignatures.IsEmpty)
					RemoveIfExists(default, block);
				else
					foreach (var parentSignature in block.ParentSignatures.AsImmutableSpan())
						RemoveIfExists(parentSignature, block);
				throw;

				void RemoveIfExists(Signature parentSignature, Block block)
				{
					if (childBlocksByParentSignature.TryGetValue(parentSignature, out var set))
						if (set.Remove(block) && set.Count == 0)
							childBlocksByParentSignature.Remove(parentSignature);
				}
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