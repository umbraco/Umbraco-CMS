using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Dynamics
{
	internal class Signature : IEquatable<Signature>
	{
		public DynamicProperty[] Properties { get; set; }
		public int HashCode { get; set; }

		public Signature(IEnumerable<DynamicProperty> properties)
		{
			this.Properties = properties.ToArray();
			HashCode = 0;
			foreach (DynamicProperty p in this.Properties)
			{
				HashCode ^= p.Name.GetHashCode() ^ p.Type.GetHashCode();
			}
		}

		public override int GetHashCode()
		{
			return HashCode;
		}

		public override bool Equals(object obj)
		{
			return obj is Signature && Equals((Signature)obj);
		}

		public bool Equals(Signature other)
		{
			if (Properties.Length != other.Properties.Length) return false;
			for (int i = 0; i < Properties.Length; i++)
			{
				if (Properties[i].Name != other.Properties[i].Name ||
				    Properties[i].Type != other.Properties[i].Type) return false;
			}
			return true;
		}
	}
}