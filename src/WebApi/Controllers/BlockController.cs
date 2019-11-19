using System;
using System.Buffers.Binary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NickStrupat;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BlockController : ControllerBase
	{
		private readonly ILogger<BlockController> logger;
		private readonly BlockDirectedAcyclicGraph blockDag;

		public BlockController(ILogger<BlockController> logger, BlockDirectedAcyclicGraph blockDag)
		{
			this.logger = logger;
			this.blockDag = blockDag;
		}

		[HttpGet]
		public IActionResult GetAllBlocks()
		{
			foreach (var block in blockDag.GetAllBlocks())
				WriteToResponseBody(block);
			return Ok();

			void WriteToResponseBody(Block block)
			{
				Span<Byte> blockLength = stackalloc Byte[sizeof(Int32)];
				BinaryPrimitives.WriteInt32BigEndian(blockLength, block.SerializationLength);
				Response.Body.Write(blockLength);
				Span<Byte> blockBytes = stackalloc Byte[block.SerializationLength];
				Response.Body.Write(blockBytes);
			}
		}

		[HttpPost]
		public IActionResult AddBlock()
		{
			var im = ImmutableMemory<Byte>.Create((Int32) Request.Body.Length, Request.Body, (span, body) => body.Read(span));
			var block = Block.Deserialize(im);
		}
	}
}
