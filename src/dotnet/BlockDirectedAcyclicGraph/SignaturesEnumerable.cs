using System;
using System.Runtime.CompilerServices;

namespace NickStrupat
{
	public readonly ref struct SignaturesEnumerable
	{
		private readonly ImmutableMemory<Byte> signatures;
		private readonly Int32 signatureLength;
		internal SignaturesEnumerable(ImmutableMemory<Byte> signatures, Int32 signatureLength)
		{
			this.signatures = signatures;
			this.signatureLength = signatureLength;
		}

		public Enumerator GetEnumerator() => new Enumerator(signatures, signatureLength);

		public ref struct Enumerator
		{
			private readonly ImmutableMemory<Byte> signatures;
			private readonly Int32 signatureLength;
			private Int32 index;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerator(ImmutableMemory<Byte> signatures, Int32 signatureLength)
			{
				this.signatures = signatures;
				this.signatureLength = signatureLength;
				index = -signatureLength;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Boolean MoveNext()
			{
				var index = this.index + signatureLength;
				if (index < signatures.Length)
				{
					this.index = index;
					return true;
				}
				return false;
			}

			public ImmutableMemory<Byte> Current {
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => signatures.Slice(index, signatureLength);
			}
		}
	}
}
