using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace TheBlockTree
{
	// no verification of block signatures nor chaining is done by the index
	public class BlockIndex
	{
		private readonly Dictionary<Bytes, (Block Block, List<Block> Children)> parentBlockAndChildrenByParentSignature =
			new Dictionary<Bytes, (Block parent, List<Block> children)>();

		public Block? GetBySignature(Bytes signature) =>
			parentBlockAndChildrenByParentSignature.GetValueOrDefault(signature).Block;

		public Block? GetParent(Block block) =>
			GetBySignature(block.ParentSignature);

		public List<Block>? GetChildren(Block block) =>
			parentBlockAndChildrenByParentSignature.GetValueOrDefault(block.Signature).Children;

		public void Add(Block block)
		{
			if (parentBlockAndChildrenByParentSignature.TryGetValue(block.sign, out var values))
				values.Add(valueToAdd);
			else
				parentBlockAndChildrenByParentSignature.Add(key, new List<V> { valueToAdd });

			//blocksBySignature.Add(block.Signature, block);
			//parentBlockAndChildrenByParentSignature.GetOrAddToList(block.ParentSignature, block);
		}

		public List<Block> GetAllBlocks() => blocksBySignature.Values.ToList();
	}
}