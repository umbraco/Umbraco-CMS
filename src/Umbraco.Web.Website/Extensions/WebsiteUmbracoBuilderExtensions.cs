using System;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static class WebsiteUmbracoBuilderExtensions
    {
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
            builder.Services.AddUnique(finder);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <typeparam name="T">The type of the site domain helper.</typeparam>
        /// <param name="builder"></param>
        public static void SetSiteDomainHelper<T>(this IUmbracoBuilder builder)
            where T : class, ISiteDomainMapper
        {
            builder.Services.AddUnique<ISiteDomainMapper, T>();
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a helper.</param>
        public static void SetSiteDomainHelper(this IUmbracoBuilder builder, Func<IServiceProvider, ISiteDomainMapper> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="helper">A helper.</param>
        public static void SetSiteDomainHelper(this IUmbracoBuilder builder, ISiteDomainMapper helper)
        {
            builder.Services.AddUnique(helper);
        }

        #endregion
    }
}
