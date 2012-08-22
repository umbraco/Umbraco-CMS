using System.Linq.Expressions;

namespace System.Linq.Dynamic
{
	internal class DynamicOrdering
	{
		public Expression Selector;
		public bool Ascending;
	}
}