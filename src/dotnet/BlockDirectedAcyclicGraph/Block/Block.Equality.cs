using System;

namespace NickStrupat
{
	public partial class Block : IEquatable<Block>
	{
		public override Boolean Equals(Object o) => o is Block block && Equals(block);
		public Boolean Equals(Block other) => EqualityComparer.Default.Equals(this, other);
		public override Int32 GetHashCode() => EqualityComparer.Default.GetHashCode(this);
	}
}