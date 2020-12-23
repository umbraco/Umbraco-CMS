using System;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Infrastructure.PublishedCache
{
    public static class CompositionExtensions
    {
        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="factory">A function creating a published snapshot service.</param>
        public static void SetPublishedSnapshotService(this IUmbracoBuilder builder, Func<IServiceProvider, IPublishedSnapshotService> factory)
        {
            builder.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <typeparam name="T">The type of the published snapshot service.</typeparam>
        /// <param name="builder">The builder.</param>
        public static void SetPublishedSnapshotService<T>(this IUmbracoBuilder builder)
            where T : class, IPublishedSnapshotService
        {
            builder.Services.AddUnique<IPublishedSnapshotService, T>();
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="service">A published snapshot service.</param>
        public static void SetPublishedSnapshotService(this IUmbracoBuilder builder, IPublishedSnapshotService service)
        {
            builder.Services.AddUnique(service);
        }
    }
}
