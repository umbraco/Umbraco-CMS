using System;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Resolves the <see cref="IRoutesCache"/> implementation.
	/// </summary>
	class RoutesCacheResolver : SingleObjectResolverBase<RoutesCacheResolver, IRoutesCache>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RoutesCacheResolve"/> class with an <see cref="IRoutesCache"/> implementation.
		/// </summary>
		/// <param name="routesCache">The <see cref="IRoutesCache"/> implementation.</param>
		internal RoutesCacheResolver(IRoutesCache routesCache)
			: base(routesCache)
		{ }

		/// <summary>
		/// Gets or sets the <see cref="IRoutesCache"/> implementation.
		/// </summary>
		public IRoutesCache RoutesCache
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
	}
}