using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    public static class RouteCollectionExtensions
	{
        /// <summary>
        /// Maps an Umbraco route with an UmbracoVirtualNodeRouteHandler
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        /// <param name="virtualNodeHandler"></param>
        /// <param name="constraints"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static Route MapUmbracoRoute(this RouteCollection routes, string name, string url, object defaults, UmbracoVirtualNodeRouteHandler virtualNodeHandler, object constraints = null, string[] namespaces = null)
        {
            var route = routes.MapRoute(name, url, defaults, constraints, namespaces);
            route.RouteHandler = virtualNodeHandler;
            return route;
        }

        /// <summary>
        /// Routes a webapi controller with namespaces
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="name"></param>
        /// <param name="url"></param>
        /// <param name="defaults"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public static Route MapHttpRoute(this RouteCollection routes, string name, string url, object defaults, string[] namespaces)
        {
            var apiRoute = routes.MapHttpRoute(
                name,
                url,
                defaults);

            //web api routes don't set the data tokens object
            if (apiRoute.DataTokens == null)
            {
                apiRoute.DataTokens = new RouteValueDictionary();
            }
            apiRoute.DataTokens.Add("Namespaces", namespaces); //look in this namespace to create the controller
            apiRoute.DataTokens.Add("UseNamespaceFallback", false); //Don't look anywhere else except this namespace!

            return apiRoute;
        }

		public static void IgnoreStandardExclusions(this RouteCollection routes)
		{
			// Ignore standard stuff...
			using (routes.GetWriteLock())
			{
				var exclusions = new Dictionary<string, object>()
					{
						{"{resource}.axd/{*pathInfo}", null},
						{"{*allaxd}", new { allaxd = @".*\.axd(/.*)?" }},
						{"{*allashx}", new { allashx = @".*\.ashx(/.*)?" }},
						{"{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" }},
						{"{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" }},
					};
				//ensure they're not re-added
				foreach (var e in exclusions.Where(e => !routes.OfType<Route>().Any(x => x.Url == e.Key)))
				{
					if (e.Value == null)
					{
						routes.IgnoreRoute(e.Key);
					}
					else
					{
						routes.IgnoreRoute(e.Key, e.Value);
					}
				}
			}
		}

		/// <summary>
		/// Extension method to manually regsiter an area
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="routes"></param>
		public static void RegisterArea<T>(this RouteCollection routes)
			where T : AreaRegistration, new()
		{

			// instantiate the area registration
		    var area = new T();

			// create a context, which is just the name and routes collection
			var context = new AreaRegistrationContext(area.AreaName, routes);

			// register it!
			area.RegisterArea(context);
		}

		///// <summary>
		///// Extension method to manually regsiter an area from the container
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="routes"></param>
		///// <param name="container"></param>
		//public static void RegisterArea<T>(this RouteCollection routes, Framework.DependencyManagement.IDependencyResolver container)
		//    where T : AreaRegistration
		//{

		//    var area = container.Resolve<T>() as AreaRegistration;
		//    if (area == null)
		//    {
		//        throw new InvalidCastException("Could not resolve type " + typeof(T).FullName + " to AreaRegistration");
		//    }

		//    // create a context, which is just the name and routes collection
		//    var context = new AreaRegistrationContext(area.AreaName, routes);

		//    // register it!
		//    area.RegisterArea(context);
		//}


		public static void RegisterArea<T>(this RouteCollection routes, T area) where T : AreaRegistration
		{
			area.RegisterArea(new AreaRegistrationContext(area.AreaName, routes));
		}

	}
}