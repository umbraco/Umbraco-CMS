using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A resolver for storing IFilteredControllerFactories
	/// </summary>
	internal sealed class FilteredControllerFactoriesResolver : ManyObjectsResolverBase<FilteredControllerFactoriesResolver, IFilteredControllerFactory>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="factories"></param>		
		internal FilteredControllerFactoriesResolver(IEnumerable<Type> factories)
			: base(factories)
		{

		}

		public IEnumerable<IFilteredControllerFactory> Factories
		{
			get { return Values; }
		}
	}
}