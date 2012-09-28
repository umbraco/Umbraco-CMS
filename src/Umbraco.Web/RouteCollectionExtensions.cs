using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web
{
	internal static class RouteCollectionExtensions
	{

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
			where T : AreaRegistration
		{

			// instantiate the area registration
			var area = Activator.CreateInstance<T>();

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