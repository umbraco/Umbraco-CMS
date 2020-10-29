using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Infrastructure.PublishedCache
{
    public static class CompositionExtensions
    {
        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a published snapshot service.</param>
        public static void SetPublishedSnapshotService(this Composition composition, Func<IServiceProvider, IPublishedSnapshotService> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <typeparam name="T">The type of the published snapshot service.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetPublishedSnapshotService<T>(this Composition composition)
            where T : IPublishedSnapshotService
        {
            composition.RegisterUnique<IPublishedSnapshotService, T>();
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="service">A published snapshot service.</param>
        public static void SetPublishedSnapshotService(this Composition composition, IPublishedSnapshotService service)
        {
            composition.RegisterUnique(_ => service);
        }
    }
}
