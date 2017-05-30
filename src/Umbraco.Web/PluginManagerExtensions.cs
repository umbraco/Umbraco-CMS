using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Core.Media;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using umbraco;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{
        /// <summary>
        /// Returns all available IAction in application
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Type> ResolveActions(this TypeLoader resolver)
        {
            return resolver.GetTypes<IAction>();
        }
        /// <summary>
        /// Returns all available TreeApiController's in application that are attribute with TreeAttribute
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> ResolveAttributedTreeControllers(this TypeLoader resolver)
        {
            return resolver.GetTypesWithAttribute<TreeController, TreeAttribute>();
        }

        internal static IEnumerable<Type> ResolveSurfaceControllers(this TypeLoader resolver)
		{
			return resolver.GetTypes<SurfaceController>();
		}

        internal static IEnumerable<Type> ResolveUmbracoApiControllers(this TypeLoader resolver)
        {
            return resolver.GetTypes<UmbracoApiController>();
        }

		/// <summary>
		/// Returns all available ITrees in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveTrees(this TypeLoader resolver)
		{
			return resolver.GetTypes<BaseTree>();
		}


		/// <summary>
		/// Returns all classes attributed with XsltExtensionAttribute attribute
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveXsltExtensions(this TypeLoader resolver)
		{
			return resolver.GetAttributedTypes<Umbraco.Core.Macros.XsltExtensionAttribute>();
		}

		/// <summary>
		/// Returns all IThumbnailProvider classes
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveThumbnailProviders(this TypeLoader resolver)
		{
			return resolver.GetTypes<IThumbnailProvider>();
		}

        /// <summary>
        /// Returns all IImageUrlProvider classes
        /// </summary>
        /// <param name="resolver"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> ResolveImageUrlProviders(this TypeLoader resolver)
        {
            return resolver.GetTypes<IImageUrlProvider>();
        }

    }
}