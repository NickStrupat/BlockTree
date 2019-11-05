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
			using var key = Key.Create(SignatureAlgorithm.Ed25519);
			var blockTree = new BlockTree(Array.Empty<Byte>(), key);

			Assert.Equal(Array.Empty<Byte>(), blockTree.Root.ParentSignature);
			Assert.NotEqual(Array.Empty<Byte>(), blockTree.Root.Signature);

			Assert.True(blockTree.TryAdd(blockTree.Root, Encoding.UTF8.GetBytes("Hello"), key, out var newBlock));
			Assert.Equal(blockTree.Root.Signature, newBlock!.ParentSignature, ReadOnlyMemoryEqualityComparer<Byte>.Instance);
		}

		[Fact]
		public void TestBlockIndex()
		{
			using var key = Key.Create(SignatureAlgorithm.Ed25519);
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

			var blocks = blockIndex.GetAllBlocks();
			var blockTree2 = new BlockTree(blocks);
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

		[Fact]
		public void Conversation()
		{
			using var userA = Key.Create(SignatureAlgorithm.Ed25519);
			using var userB = Key.Create(SignatureAlgorithm.Ed25519);

			static Byte[] MsgBytes(String msg) => Encoding.ASCII.GetBytes($"{DateTime.Now.ToString()}: {msg}");
			var tree = new BlockTree(MsgBytes("hello"), userA);
			Assert.True(tree.TryAdd(tree.Root, MsgBytes("hi"), userB, out var postedMessage));
			Assert.True(tree.TryAdd(postedMessage!, MsgBytes("how are you?"), userA, out postedMessage));
			Assert.True(tree.TryAdd(postedMessage!, MsgBytes("i am well"), userB, out postedMessage));
			Assert.True(tree.TryAdd(postedMessage!, MsgBytes("(read)"), userB, out postedMessage));

			var msgs = new List<String>();
			foreach (var (Block, Level) in tree.TraverseDepthFirst())
				msgs.Add(Convert.ToBase64String(Block.PublicKey.Span).Substring(0, 4) + ": " + Encoding.ASCII.GetString(Block.Data.Span));
		}
	}
}
