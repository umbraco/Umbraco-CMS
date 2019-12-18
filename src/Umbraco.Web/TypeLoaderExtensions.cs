using System;
using System.Collections.Generic;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Core.Composing;


namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for the <see cref="TypeLoader"/> class.
    /// </summary>
    public static class TypeLoaderExtensions
    {
        /// <summary>
        /// Gets all types implementing <see cref="SurfaceController"/>.
        /// </summary>
        internal static IEnumerable<Type> GetSurfaceControllers(this TypeLoader typeLoader)
            => typeLoader.GetTypes<SurfaceController>();

        /// <summary>
        /// Gets all types implementing <see cref="UmbracoApiController"/>.
        /// </summary>
        internal static IEnumerable<Type> GetUmbracoApiControllers(this TypeLoader typeLoader)
            => typeLoader.GetTypes<UmbracoApiController>();
    }
}
