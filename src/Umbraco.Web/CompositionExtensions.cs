using System;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Infrastructure.Composing;
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
// Umbraco.Core's own CompositionExtensions class

// ReSharper disable once CheckNamespace
namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class WebCompositionExtensions
    {
        [Obsolete("This extension method exists only to ease migration, please refactor")]
        public static IServiceProvider CreateServiceProvider(this Composition composition)
        {
            composition.RegisterBuilders();
            return composition.Services.BuildServiceProvider();
        }

        #region Uniques

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <typeparam name="T">The type of the content last chance finder.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetContentLastChanceFinder<T>(this Composition composition)
            where T : IContentLastChanceFinder
        {
            composition.RegisterUnique<IContentLastChanceFinder, T>();
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, Func<IServiceProvider, IContentLastChanceFinder> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="finder">A last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, IContentLastChanceFinder finder)
        {
            composition.RegisterUnique(_ => finder);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <typeparam name="T">The type of the site domain helper.</typeparam>
        /// <param name="composition"></param>
        public static void SetSiteDomainHelper<T>(this Composition composition)
            where T : ISiteDomainHelper
        {
            composition.RegisterUnique<ISiteDomainHelper, T>();
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, Func<IServiceProvider, ISiteDomainHelper> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="helper">A helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, ISiteDomainHelper helper)
        {
            composition.RegisterUnique(_ => helper);
        }

        #endregion
    }
}
