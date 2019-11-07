using System;
using System.IO;
using System.Text;
using NickStrupat;
using NSec.Cryptography;

namespace SerializationTesting
{
	class Program
	{
		private const String Path = "block.bin";

		static void Main(string[] args)
		{
			if (File.Exists(Path))
			{
				var rawBlockBytes = File.ReadAllBytes(Path);
				var block = Block.Deserialize(new ImmutableMemory<Byte>(rawBlockBytes));
				Console.WriteLine(block.Verify());
			}

			//using var key = Key.Create(SignatureAlgorithm.Ed25519);

			//var request = new Block(ImmutableMemory<Byte>.Empty, Encoding.UTF8.GetBytes("request"), key);
			//var bytes = new Byte[request.SerializationLength];
			//request.Serialize(bytes);
			//File.WriteAllBytes(Path, bytes);
		}
	}
}
