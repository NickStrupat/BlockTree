using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TheBlockTree
{
	// no verification of block signatures nor chaining is done by the index
	public class BlockIndex
	{
		private readonly Dictionary<ReadOnlyMemory<Byte>, Block> blocksBySignature =
			new Dictionary<ReadOnlyMemory<Byte>, Block>(ReadOnlyMemoryEqualityComparer<Byte>.Instance);
		private readonly Dictionary<ReadOnlyMemory<Byte>, List<Block>> childBlocksByParentSignature =
			new Dictionary<ReadOnlyMemory<Byte>, List<Block>>(ReadOnlyMemoryEqualityComparer<Byte>.Instance);

		public Int32 Count => blocksBySignature.Count;

		public Boolean Contains(ReadOnlyMemory<Byte> signature) =>
			blocksBySignature.ContainsKey(signature);

		public Boolean TryGetBySignature(ReadOnlyMemory<Byte> signature, [NotNullWhen(true)] out Block? block) =>
			blocksBySignature.TryGetValue(signature, out block);

		public Boolean TryGetChildren(ReadOnlyMemory<Byte> signature, [NotNullWhen(true)] out IReadOnlyList<Block>? children)
		{
			if (childBlocksByParentSignature.TryGetValue(signature, out var values))
			{
				children = values.AsReadOnly();
				return true;
			}
			children = null;
			return false;
		}

		public void Add(Block block)
		{
			blocksBySignature.Add(block.Signature, block);
			childBlocksByParentSignature.AddValue(block.ParentSignature, block);
		}

		public List<Block> GetAllBlocks() =>
			blocksBySignature.Values.ToList();
	}
}