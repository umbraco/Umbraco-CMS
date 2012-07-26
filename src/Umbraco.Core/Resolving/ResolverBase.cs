using System;
using System.Threading;

namespace Umbraco.Core.Resolving
{
	/// <summary>
	/// base class for resolvers which declare a singleton accessor
	/// </summary>
	/// <typeparam name="TResolver"></typeparam>
	internal abstract class ResolverBase<TResolver> 
		where TResolver : class
	{
		public static TResolver Current { get; set; }
	}
}
