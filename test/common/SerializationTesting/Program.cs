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
			using var key = Key.Create(SignatureAlgorithm.Ed25519);

			var text = "request";
			static void StringToBytes(Span<Byte> bytes, String s) => Encoding.UTF8.GetBytes(s, bytes);
			var data = ImmutableMemory<Byte>.Create(Encoding.UTF8.GetByteCount(text), text, StringToBytes);
			var request = new Block(data, key);
			var bytes = new Byte[request.SerializationLength];
			request.Serialize(bytes);
			//File.WriteAllBytes(Path, bytes);

			//if (File.Exists(Path))
			//{
				//var rawBlockBytes = File.ReadAllBytes(Path);
				var block = Block.Deserialize(new ImmutableMemory<Byte>(bytes));
				//Console.WriteLine(block.Verify());
			//}
			if (!request.Equals(block))
				throw new Exception();
		}
	}
}
