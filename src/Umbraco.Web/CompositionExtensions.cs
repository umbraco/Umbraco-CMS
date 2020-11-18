using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Builder;
using Umbraco.Core.Composing;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Dashboards;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Sections;
using Umbraco.Web.Tour;
using Umbraco.Web.Trees;
using Current = Umbraco.Web.Composing.Current;

// the namespace here is intentional -  although defined in Umbraco.Web assembly,
// this class should be visible when using Umbraco.Core.Components, alongside
// Umbraco.Core's own IUmbracoBuilderExtensions class

// ReSharper disable once CheckNamespace
namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static class WebIUmbracoBuilderExtensions
    {
        [Obsolete("This extension method exists only to ease migration, please refactor")]
        public static IServiceProvider CreateServiceProvider(this IUmbracoBuilder builder)
        {
            builder.Build();
            return builder.Services.BuildServiceProvider();
        }

        #region Uniques

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <typeparam name="T">The type of the content last chance finder.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetContentLastChanceFinder<T>(this IUmbracoBuilder builder)
            where T : class, IContentLastChanceFinder
        {
            builder.Services.AddUnique<IContentLastChanceFinder, T>();
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a last chance finder.</param>
        public static void SetContentLastChanceFinder(this IUmbracoBuilder builder, Func<IServiceProvider, IContentLastChanceFinder> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="finder">A last chance finder.</param>
        public static void SetContentLastChanceFinder(this IUmbracoBuilder builder, IContentLastChanceFinder finder)
        {
            builder.Services.AddUnique(_ => finder);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <typeparam name="T">The type of the site domain helper.</typeparam>
        /// <param name="builder"></param>
        public static void SetSiteDomainHelper<T>(this IUmbracoBuilder builder)
            where T : class, ISiteDomainHelper
        {
            builder.Services.AddUnique<ISiteDomainHelper, T>();
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a helper.</param>
        public static void SetSiteDomainHelper(this IUmbracoBuilder builder, Func<IServiceProvider, ISiteDomainHelper> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="helper">A helper.</param>
        public static void SetSiteDomainHelper(this IUmbracoBuilder builder, ISiteDomainHelper helper)
        {
            builder.Services.AddUnique(_ => helper);
        }

        #endregion
    }
}
