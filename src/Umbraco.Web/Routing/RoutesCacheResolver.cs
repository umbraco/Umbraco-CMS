using System;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Resolves the <see cref="IRoutesCache"/> implementation.
	/// </summary>
	internal sealed class RoutesCacheResolver : SingleObjectResolverBase<RoutesCacheResolver, IRoutesCache>
	{
		
		/// <summary>
		/// Initializes a new instance of the <see cref="RoutesCacheResolver"/> class with an <see cref="IRoutesCache"/> implementation.
		/// </summary>
		/// <param name="routesCache">The <see cref="IRoutesCache"/> implementation.</param>
		internal RoutesCacheResolver(IRoutesCache routesCache)
			: base(routesCache)
		{ }


		/// <summary>
		/// Can be used by developers at runtime to set their IRoutesCache at app startup
		/// </summary>
		/// <param name="routesCache"></param>
		public void SetRoutesCache(IRoutesCache routesCache)
		{
			Value = routesCache;
		}

		/// <summary>
		/// Gets or sets the <see cref="IRoutesCache"/> implementation.
		/// </summary>
		public IRoutesCache RoutesCache
		{
			get { return this.Value; }
		}
	}
}