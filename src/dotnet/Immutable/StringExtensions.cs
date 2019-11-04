using System;
using System.Collections.Generic;
using System.Text;

namespace NickStrupat
{
	public static class StringExtensions
	{
		public static ImmutableMemory<Char> AsImmutableMemory(this String @string) =>
			new ImmutableMemory<Char>(@string.AsMemory());

		public static ImmutableMemory<Byte> AsImmutableMemory(this String @string, Encoding encoding) =>
			new ImmutableMemory<Byte>(encoding.GetBytes(@string).AsMemory());
	}
}
