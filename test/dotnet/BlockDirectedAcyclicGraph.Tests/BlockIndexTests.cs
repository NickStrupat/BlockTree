using NickStrupat;
using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BlockDirectedAcyclicGraph_Tests
{
	public class BlockIndexTests
	{

		[Fact]
		public void TestBlockIndex()
		{
			using var key = Key.Create(SignatureAlgorithm.Ed25519);
			var blockDag = new BlockDirectedAcyclicGraph(Array.Empty<Block>());
			var root = new Block(ImmutableMemory<Byte>.Empty, key);
			blockDag.Add(root);
			var verifiedBlocks = new List<Block>();
			for (var i = 0; i != 3; i++)
			{
				Assert.True(
					blockDag.TryAdd(root, Encoding.UTF8.GetBytes($"Hello #{i}").ToImmutableMemory(), key, out var newBlock)
				);
				verifiedBlocks.Add(newBlock);
			}

			var blockIndex = new BlockIndex();
			foreach (var verifiedBlock in verifiedBlocks)
				blockIndex.Add(verifiedBlock);
			blockIndex.Add(root);

			var blocks = blockIndex.GetAllBlocks();
			var blockTree2 = new BlockDirectedAcyclicGraph(blocks);
		}

		[Fact]
		public void TestBlockIndexSingleRoot()
		{
			Assert.ThrowsAny<ArgumentException>(() =>
			{
				using var key = Key.Create(SignatureAlgorithm.Ed25519);
				var blockIndex = new BlockIndex();
				var root = new Block(ImmutableMemory<Byte>.Empty, key);
				blockIndex.Add(root);
				for (var i = 0; i != 2; i++)
				{
					var blockTree = new BlockDirectedAcyclicGraph(Enumerable.Empty<Block>());
					for (var j = 0; j != 3; j++)
					{
						Assert.True(
							blockTree.TryAdd(root, Encoding.UTF8.GetBytes($"Hello #{j}").ToImmutableMemory(), key, out var newBlock)
						);
						blockIndex.Add(newBlock);
					}
				}
			});
		}
	}
}
