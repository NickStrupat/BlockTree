using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

		public Block? GetBySignature(ReadOnlyMemory<Byte> signature)
		{
			Console.WriteLine("a\t" + new Bytes(signature));
			return blocksBySignature.GetValueOrDefault(signature);
		}

		public Block? GetParent(Block block) =>
			GetBySignature(block.ParentSignature);

		public IReadOnlyList<Block>? GetChildren(ReadOnlyMemory<Byte> signature) =>
			childBlocksByParentSignature.GetValueOrDefault(signature);

		public IReadOnlyList<Block>? GetChildren(Block block) =>
			childBlocksByParentSignature.GetValueOrDefault(block.Signature);

		public void Add(Block block)
		{
			blocksBySignature.Add(block.Signature, block);
			if (childBlocksByParentSignature.TryGetValue(block.ParentSignature, out var children))
				children.Add(block);
			else
				childBlocksByParentSignature.Add(block.ParentSignature, new List<Block> { block });
		}

		public List<Block> GetAllBlocks() =>
			blocksBySignature.Values.ToList();
	}
}