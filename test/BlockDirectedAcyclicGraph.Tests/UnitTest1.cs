using NickStrupat;
using NSec.Cryptography;
using System;
using System.Text;
using Xunit;

namespace BlockDirectedAcyclicGraph.Tests
{
	public class UnitTest1
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

			var genesisBlock = new Block(ImmutableMemory<Byte>.Empty, Encoding.Unicode.GetBytes("genesis"), key1);
			var block = new Block(genesisBlock.Signature, Encoding.Unicode.GetBytes("request"), key2);

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
		}
	}
}
