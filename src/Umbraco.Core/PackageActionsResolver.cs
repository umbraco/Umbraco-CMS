using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IPackageAction objects
	/// </summary>
	internal sealed class PackageActionsResolver : ManyObjectsResolverBase<PackageActionsResolver, IPackageAction>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="packageActions"></param>		
		internal PackageActionsResolver(IEnumerable<Type> packageActions)
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