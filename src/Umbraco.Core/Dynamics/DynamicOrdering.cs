using System.Linq.Expressions;

namespace Umbraco.Core.Dynamics
{
	internal class DynamicOrdering
	{
		public Expression Selector;
		public bool Ascending;
	}
}