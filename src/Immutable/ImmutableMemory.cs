using System;
using System.Buffers;
using System.ComponentModel;

namespace BlockTree
{
	public readonly struct ImmutableMemory<T>
	{
		public ReadOnlyMemory<T> Memory { get; }
		public ImmutableMemory(ReadOnlySpan<T> source) => Memory = source.IsEmpty ? ReadOnlyMemory<T>.Empty : source.ToArray();
		private ImmutableMemory(ReadOnlyMemory<T> memory) => Memory = memory;

		public ImmutableSpan<T> ImmutableSpan => new ImmutableSpan<T>(this);

		public static ImmutableMemory<T> Empty { get; } = new ImmutableMemory<T>(ReadOnlySpan<T>.Empty);

		public Int32 Length => Memory.Length;
		public Boolean IsEmpty => Memory.IsEmpty;

		public void CopyTo(Memory<T> destination) => Memory.CopyTo(destination);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Boolean Equals(Object obj) => obj is ImmutableMemory<T> im && Memory.Equals(im);
		public Boolean Equals(ImmutableMemory<T> other) => Memory.Equals(other.Memory);
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Int32 GetHashCode() => Memory.GetHashCode();
		public MemoryHandle Pin() => Memory.Pin();
		public ImmutableMemory<T> Slice(Int32 start, Int32 length) => new ImmutableMemory<T>(Memory.Slice(start, length));
		public ImmutableMemory<T> Slice(Int32 start) => new ImmutableMemory<T>(Memory.Slice(start));
		public T[] ToArray() => Memory.ToArray();
		public override String ToString() => Memory.ToString();
		public Boolean TryCopyTo(Memory<T> destination) => Memory.TryCopyTo(destination);

		public static implicit operator ImmutableMemory<T>(ArraySegment<T> segment) => new ImmutableMemory<T>(segment.AsSpan());
		public static implicit operator ImmutableMemory<T>(T[] array) => new ImmutableMemory<T>((ReadOnlySpan<T>) array);
	}
}
