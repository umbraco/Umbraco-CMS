using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Interfaces;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.interfaces;
using umbraco.presentation.umbracobase;


namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{
		/// <summary>
		/// Returns all available ITrees in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveTrees(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<ITree>();
		}

		/// <summary>
		/// Returns all classes attributed with RestExtension attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveRestExtensions(this PluginTypeResolver resolver)
		{
			return resolver.ResolveAttributedTypes<RestExtension>();
		}

		/// <summary>
		/// Returns all classes attributed with XsltExtensionAttribute attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveXsltExtensions(this PluginTypeResolver resolver)
		{
			return resolver.ResolveAttributedTypes<XsltExtensionAttribute>();
		}

		/// <summary>
		/// Returns all IThumbnailProvider classes
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveThumbnailProviders(this PluginTypeResolver resolver)
		{
			return resolver.ResolveTypes<IThumbnailProvider>();
		}
	}
}