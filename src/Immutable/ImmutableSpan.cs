using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlockTree
{
	public readonly ref struct ImmutableSpan<T>
	{
		public ReadOnlySpan<T> Span { get; }
		public ImmutableSpan(ImmutableMemory<T> immutableMemory) => Span = immutableMemory.Memory.Span;
		private ImmutableSpan(ReadOnlySpan<T> span) => Span = span;
		public static ImmutableSpan<T> Empty => new ImmutableSpan<T>(ImmutableMemory<T>.Empty);

		public ref readonly T this[Int32 index] => ref Span[index];

		public Int32 Length => Span.Length;
		public Boolean IsEmpty => Span.IsEmpty;

		public void CopyTo(Span<T> destination) => Span.CopyTo(destination);

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Equals() on ReadOnlySpan will always throw an exception. Use == instead.")]
		public override Boolean Equals(Object obj) => throw new NotSupportedException();

		public Enumerator GetEnumerator() => new Enumerator(this);

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("GetHashCode() on ReadOnlySpan will always throw an exception.")]
		public override Int32 GetHashCode() => throw new NotSupportedException();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ref readonly T GetPinnableReference() => ref Span.GetPinnableReference();
		public ImmutableSpan<T> Slice(Int32 start) => new ImmutableSpan<T>(Span.Slice(start));
		public ImmutableSpan<T> Slice(Int32 start, Int32 length) => new ImmutableSpan<T>(Span.Slice(start, length));
		
		public T[] ToArray() => Span.ToArray();
		public override String ToString() => Span.ToString();
		public Boolean TryCopyTo(Span<T> destination) => Span.TryCopyTo(destination);
		
		public static Boolean operator ==(ImmutableSpan<T> left, ImmutableSpan<T> right) => left.Span == right.Span;
		public static Boolean operator !=(ImmutableSpan<T> left, ImmutableSpan<T> right) => left.Span == right.Span;

		public static implicit operator ImmutableSpan<T>(T[] array) => new ImmutableSpan<T>((ImmutableMemory<T>) array);
		public static implicit operator ImmutableSpan<T>(ArraySegment<T> segment) => new ImmutableSpan<T>((ImmutableMemory<T>) segment);

		public ref struct Enumerator
		{
			private readonly ImmutableSpan<T> span;
			private int index;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			internal Enumerator(ImmutableSpan<T> span)
			{
				this.span = span;
				index = -1;
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				int index = this.index + 1;
				if (index < span.Length)
				{
					this.index = index;
					return true;
				}

				return false;
			}

			public ref readonly T Current {
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get => ref span[index];
			}
		}
	}
}
