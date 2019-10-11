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

			Assert.Equal(Array.Empty<Byte>(), blockTree.Root.ParentSignature.Value);
			Assert.NotEqual(Array.Empty<Byte>(), blockTree.Root.Signature.Value);

			Assert.True(blockTree.TryAdd(blockTree.Root, Encoding.UTF8.GetBytes("Hello"), key, out var newBlock));
			Assert.Equal(blockTree.Root.Signature, newBlock!.ParentSignature);
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
					blockTree.TryAdd(blockTree.Root, Encoding.UTF8.GetBytes($"Hello #{i}"), key, out var newBlock)
				);
				verifiedBlocks.Add(newBlock!);
			}

			var blockIndex = new BlockIndex();
			foreach (var verifiedBlock in verifiedBlocks)
				blockIndex.Add(verifiedBlock);
			blockIndex.Add(blockTree.Root);

			var blockTree2 = new BlockTree(blockIndex.GetAllBlocks());
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
							blockTree.TryAdd(blockTree.Root, Encoding.UTF8.GetBytes($"Hello #{j}"), key, out var newBlock)
						);
						blockIndex.Add(newBlock!);
					}
				}
			});
		}
	}
}
