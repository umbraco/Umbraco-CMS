using System;

namespace Umbraco.Web.Routing
{
	// this is a bi-directional cache that contains
	// - nodeId to route (used for NiceUrl)
	// - route to nodeId (used for inbound requests)
	//
	// a route is [rootId]/path/to/node
	// where rootId is the id of the "site root" node
	//
	internal class RoutesCache
	{
		private static readonly RoutesCache Instance = new RoutesCache();
		private static IRoutesCache _provider;

		/// <summary>
		/// public contructor assigns the DefaultRoutesCache as the default provider
		/// </summary>
		public RoutesCache()
			:this(null)
		{			
		}

		/// <summary>
		/// Constructor sets a custom provider if specified
		/// </summary>
		/// <param name="provider"></param>
		internal RoutesCache(IRoutesCache provider)
		{
			_provider = provider ?? new DefaultRoutesCache();
		}

		/// <summary>
		/// Set the routes cache provider
		/// </summary>
		/// <param name="provider"></param>
		public static void SetProvider(IRoutesCache provider)
		{
			if (provider == null) throw new ArgumentNullException("provider");
			_provider = provider;
		}

		/// <summary>
		/// Singleton accessor
		/// </summary>
		public static RoutesCache Current
		{
			get { return Instance; }
		}

		/// <summary>
		/// Get the current provider
		/// </summary>
		/// <returns></returns>
		public IRoutesCache GetProvider()
		{
			return _provider;
		}
	}
}