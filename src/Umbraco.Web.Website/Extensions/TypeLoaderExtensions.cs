﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Website.Controllers;


namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="TypeLoader"/> class.
    /// </summary>
    // Migrated to .NET Core
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
