using NickStrupat;
using NSec.Cryptography;
using System;
using System.Text;
using Xunit;

namespace BlockDirectedAcyclicGraph_Tests
{
	public class BlockTests
	{
		[Fact]
		public void DefaultBlockIsNotVerified()
		{
			var block = new Block();
			Assert.False(block.Verify());

			block = default;
			Assert.False(block.Verify());
		}

		[Fact]
		public void NewBlockIsVerified()
		{
			var key = Key.Create(Block.Algorithm);
			var block = new Block(ImmutableMemory<Byte>.Empty, ImmutableMemory<Byte>.Empty, key);
			Assert.True(block.Verify());
		}

		[Fact]
		public void SerializationRoundtripEquality()
		{
			var key = Key.Create(Block.Algorithm);

			var requestBlock = new Block(ImmutableMemory<Byte>.Empty, Encoding.UTF8.GetBytes("request"), key);
			var responeBlock = new Block(requestBlock.Signature, Encoding.UTF8.GetBytes("response"), key);

			var reqBytes = ImmutableMemory<Byte>.Create(requestBlock.SerializationLength, requestBlock, (bytes, block) => block.Serialize(bytes));
			var resBytes = ImmutableMemory<Byte>.Create(responeBlock.SerializationLength, requestBlock, (bytes, block) => block.Serialize(bytes));

			var requestBlock2 = Block.Deserialize(reqBytes);
			var responeBlock2 = Block.Deserialize(resBytes);

			Assert.True(requestBlock2.Verify());
			Assert.True(responeBlock2.Verify());

			Assert.Equal(requestBlock.GetHashCode(), requestBlock2.GetHashCode());
			Assert.Equal(responeBlock2.GetHashCode(), responeBlock2.GetHashCode());

			Assert.Equal(requestBlock, requestBlock2);
			Assert.Equal(responeBlock2, responeBlock2);
		}

		[Fact]
		public void VerifiedGenesisBlockSerializedThenDeserializedIsVerified()
		{
			var key = Key.Create(Block.Algorithm);
			var block = new Block(ImmutableMemory<Byte>.Empty, ImmutableMemory<Byte>.Empty, key);
			Assert.True(block.Verify());

			Span<Byte> bytes = stackalloc Byte[block.SerializationLength];
			block.Serialize(bytes);

			Assert.True(Block.TryDeserialize(bytes.ToArray(), out var block2));
			Assert.True(block2.Verify());
		}

		[Fact]
		public void VerifiedBlockSerializedThenDeserializedIsVerified()
		{
			var key1 = Key.Create(Block.Algorithm);
			var key2 = Key.Create(Block.Algorithm);

			var genesisBlock = new Block(ImmutableMemory<Byte>.Empty, Encoding.UTF8.GetBytes("genesis"), key1);
			var block = new Block(genesisBlock.Signature, Encoding.UTF8.GetBytes("request"), key2);

			Span<Byte> bytes = stackalloc Byte[genesisBlock.SerializationLength];
			Span<Byte> bytes2 = stackalloc Byte[block.SerializationLength];
			genesisBlock.Serialize(bytes);
			block.Serialize(bytes2);

			Assert.True(Block.TryDeserialize(bytes.ToArray(), out var genesisBlock2));
			Assert.True(genesisBlock2.Verify());

			Assert.True(Block.TryDeserialize(bytes2.ToArray(), out var block2));
			Assert.True(block2.Verify());

		}

		[Fact]
		public void MultiParentBlockIsVerified()
		{
			static ImmutableMemory<Byte> GetBytes(String s) => s.AsImmutableMemory(Encoding.UTF8);

			var key1 = Key.Create(Block.Algorithm);
			var key2 = Key.Create(Block.Algorithm);
			var key3 = Key.Create(Block.Algorithm);

			var genesisBlock = new Block(ImmutableMemory<Byte>.Empty, GetBytes("genesis"), key1);
			var block2 = new Block(genesisBlock.Signature, GetBytes("request"), key2);
			var block3 = new Block(block2.Signature, GetBytes("response"), key1);

			Span<Byte> parentSignatures = stackalloc Byte[Block.Algorithm.SignatureSize * 2];
			block2.Signature.ImmutableSpan.CopyTo(parentSignatures);
			block3.Signature.ImmutableSpan.CopyTo(parentSignatures.Slice(Block.Algorithm.SignatureSize));
			var joinBlock = new Block(new ImmutableMemory<Byte>(parentSignatures), GetBytes("join"), key3);
			Assert.True(joinBlock.Verify());

			foreach (var block in new[] { genesisBlock, block2, block3, joinBlock })
			{
				var memory = ImmutableMemory<Byte>.Create(genesisBlock.SerializationLength, genesisBlock, (buf, blk) => blk.Serialize(buf));
				var deserializedBlock = Block.Deserialize(memory);
				Assert.True(deserializedBlock.Verify());
			}
		}
	}
}
