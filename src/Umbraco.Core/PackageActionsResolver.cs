using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IPackageAction objects
	/// </summary>
	internal sealed class PackageActionsResolver : LazyManyObjectsResolverBase<PackageActionsResolver, IPackageAction>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="packageActions"></param>		
	    internal PackageActionsResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> packageActions)
            : base(serviceProvider, logger, packageActions)
		{
			
		}

		/// <summary>
		/// Gets the <see cref="IPackageAction"/> implementations.
		/// </summary>
		public IEnumerable<IPackageAction> PackageActions
		{
			get
			{
				return Values;
			}
		}

	}
}