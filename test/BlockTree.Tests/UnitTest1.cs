using System;
using System.Collections.Generic;
using System.Text;
using NSec.Cryptography;
using Xunit;

namespace TheBlockTree.BlockTreeTests
{
	public class UnitTest1
	{
		[Fact]
		public void TestBlockTree()
		{
			var key = Key.Create(SignatureAlgorithm.Ed25519);
			var blockTree = new BlockTree(Array.Empty<Byte>(), key);

			Assert.Equal(Array.Empty<Byte>(), blockTree.Root.Value.ParentSignature);
			Assert.NotEqual(Array.Empty<Byte>(), blockTree.Root.Value.Signature);

			Assert.True(blockTree.TryAdd(blockTree.Root.Value, Encoding.UTF8.GetBytes("Hello"), key, out var newBlock));
			Assert.Equal(blockTree.Root.Value.Signature, newBlock!.ParentSignature, ReadOnlyMemoryEqualityComparer<Byte>.Instance);
		}

		[Fact]
		public void TestBlockIndex()
		{
			var key = Key.Create(SignatureAlgorithm.Ed25519);
			var blockTree = new BlockTree(Array.Empty<Byte>(), key);
			var verifiedBlocks = new List<Block>();
			for (var i = 0; i != 3; i++)
			{
				Assert.True(
					blockTree.TryAdd(blockTree.Root.Value, Encoding.UTF8.GetBytes($"Hello #{i}"), key, out var newBlock)
				);
				verifiedBlocks.Add(newBlock!);
			}

			var blockIndex = new BlockIndex();
			foreach (var verifiedBlock in verifiedBlocks)
				blockIndex.Add(verifiedBlock);
			blockIndex.Add(blockTree.Root.Value);

			var blocks = blockIndex.GetAllBlocks();
			var blockTree2 = new BlockTree(blocks);
		}

		[Fact]
		public void TestBlockIndexSingleRoot()
		{
			Assert.ThrowsAny<ArgumentException>(() =>
			{
				var key = Key.Create(SignatureAlgorithm.Ed25519);
				var blockIndex = new BlockIndex();
				for (var i = 0; i != 2; i++)
				{
					var blockTree = new BlockTree(Array.Empty<Byte>(), key);
					for (var j = 0; j != 3; j++)
					{
						Assert.True(
							blockTree.TryAdd(blockTree.Root.Value, Encoding.UTF8.GetBytes($"Hello #{j}"), key, out var newBlock)
						);
						blockIndex.Add(newBlock!);
					}
				}
			});
		}
	}
}
