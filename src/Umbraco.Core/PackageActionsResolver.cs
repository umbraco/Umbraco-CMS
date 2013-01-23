using System;
using System.Collections.Generic;
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
		/// <param name="packageActions"></param>		
		internal PackageActionsResolver(Func<IEnumerable<Type>> packageActions)
			: base(packageActions)
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