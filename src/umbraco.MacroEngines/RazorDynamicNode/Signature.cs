using System.Collections.Generic;

namespace System.Linq.Dynamic
{
	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.Signature")]
	internal class Signature : Umbraco.Core.Dynamics.Signature
	{
		public Signature(IEnumerable<Umbraco.Core.Dynamics.DynamicProperty> properties) : base(properties)
		{
		}
	}
}