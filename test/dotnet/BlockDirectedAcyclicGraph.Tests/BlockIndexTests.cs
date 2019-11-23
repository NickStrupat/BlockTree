using NickStrupat;
using NSec.Cryptography;
using System;
using System.Collections.Generic;
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
			blockDag.Add()
			var verifiedBlocks = new List<Block>();
			for (var i = 0; i != 3; i++)
			{
				Assert.True(
					blockDag.TryAdd(blockDag.Root, Encoding.UTF8.GetBytes($"Hello #{i}").ToImmutableMemory(), key, out var newBlock)
				);
				verifiedBlocks.Add(newBlock);
			}

			var blockIndex = new BlockIndex();
			foreach (var verifiedBlock in verifiedBlocks)
				blockIndex.Add(verifiedBlock);
			blockIndex.Add(blockDag.Root);

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
				for (var i = 0; i != 2; i++)
				{
					var blockTree = new BlockDirectedAcyclicGraph(Array.Empty<Byte>().ToImmutableMemory(), key);
					for (var j = 0; j != 3; j++)
					{
						Assert.True(
							blockTree.TryAdd(blockTree.Root, Encoding.UTF8.GetBytes($"Hello #{j}").ToImmutableMemory(), key, out var newBlock)
						);
						blockIndex.Add(newBlock);
					}
				}
			});
		}
	}
}
