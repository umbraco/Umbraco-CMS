using System;
using System.Collections.Generic;
using Umbraco.Core.Media;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using Umbraco.Web.WebApi;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.Composing;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for the PluginTypemgr
    /// </summary>
    public static class TypeLoaderExtensions
    {
        /// <summary>
        /// Returns all available IAction in application
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<Type> GetActions(this TypeLoader mgr)
        {
            return mgr.GetTypes<IAction>();
        }

        /// <summary>
        /// Returns all available TreeApiController's in application that are attribute with TreeAttribute
        /// </summary>
        /// <param name="mgr"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetAttributedTreeControllers(this TypeLoader mgr)
        {
            return mgr.GetTypesWithAttribute<TreeController, TreeAttribute>();
        }

        internal static IEnumerable<Type> GetSurfaceControllers(this TypeLoader mgr)
        {
            return mgr.GetTypes<SurfaceController>();
        }

        internal static IEnumerable<Type> GetUmbracoApiControllers(this TypeLoader mgr)
        {
            return mgr.GetTypes<UmbracoApiController>();
        }

        /// <summary>
        /// Returns all available ITrees in application
        /// </summary>
        /// <param name="mgr"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetTrees(this TypeLoader mgr)
        {
            return mgr.GetTypes<BaseTree>();
        }

        /// <summary>
        /// Returns all available ISearchableTrees in application
        /// </summary>
        /// <param name="mgr"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> GetSearchableTrees(this TypeLoader mgr)
        {
            return mgr.GetTypes<ISearchableTree>();
        }
        
    }
}
