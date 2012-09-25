using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{
		internal static IEnumerable<Type> ResolveSurfaceControllers(this PluginManager resolver)
		{
			return resolver.ResolveTypes<SurfaceController>();
		}

		/// <summary>
		/// Returns all available ITrees in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveTrees(this PluginManager resolver)
		{
			return resolver.ResolveTypes<ITree>();
		}

		/// <summary>
		/// Returns all classes attributed with legacy RestExtension attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveLegacyRestExtensions(this PluginManager resolver)
		{
			return resolver.ResolveAttributedTypes<global::umbraco.presentation.umbracobase.RestExtension>();
		}

		/// <summary>
		/// Returns all classes attributed with RestExtensionAttribute attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveRestExtensions(this PluginManager resolver)
		{
			return resolver.ResolveAttributedTypes<Umbraco.Web.BaseRest.RestExtensionAttribute>();
		}

		/// <summary>
		/// Returns all classes attributed with XsltExtensionAttribute attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveXsltExtensions(this PluginManager resolver)
		{
			return resolver.ResolveAttributedTypes<XsltExtensionAttribute>();
		}

		/// <summary>
		/// Returns all IThumbnailProvider classes
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveThumbnailProviders(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IThumbnailProvider>();
		}
	}
}