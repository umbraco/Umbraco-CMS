using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A resolver for storing IFilteredControllerFactories
	/// </summary>
	public sealed class FilteredControllerFactoriesResolver : ManyObjectsResolverBase<FilteredControllerFactoriesResolver, IFilteredControllerFactory>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="logger"></param>
	    /// <param name="factories"></param>
	    /// <param name="serviceProvider"></param>		
	    internal FilteredControllerFactoriesResolver(IServiceProvider serviceProvider, ILogger logger, IEnumerable<Type> factories)
            : base(serviceProvider, logger, factories)
		{

		}

		public IEnumerable<IFilteredControllerFactory> Factories
		{
			get { return Values; }
		}
	}
}