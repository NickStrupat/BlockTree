using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WebApi
{
	public class Program
	{
		public static void Main(String[] args) =>
			Host
				.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
				.Build()
				.Run();
	}
}
