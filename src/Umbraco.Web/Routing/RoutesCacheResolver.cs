using System;
using Umbraco.Core.Resolving;

namespace Umbraco.Web.Routing
{
	class RoutesCacheResolver : SingleResolverBase<RoutesCacheResolver, IRoutesCache>
	{
		internal RoutesCacheResolver(IRoutesCache routesCache)
			: base(routesCache)
		{ }

		public IRoutesCache RoutesCache
		{
			get { return this.Value; }
			set { this.Value = value; }
		}
	}
}