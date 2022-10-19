using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Extensions;

public static class CompositionExtensions
{
    /// <summary>
    ///     Sets the published snapshot service.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a published snapshot service.</param>
    public static IUmbracoBuilder SetPublishedSnapshotService(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IPublishedSnapshotService> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the published snapshot service.
    /// </summary>
    /// <typeparam name="T">The type of the published snapshot service.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetPublishedSnapshotService<T>(this IUmbracoBuilder builder)
        where T : class, IPublishedSnapshotService
    {
        builder.Services.AddUnique<IPublishedSnapshotService, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the published snapshot service.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="service">A published snapshot service.</param>
    public static IUmbracoBuilder SetPublishedSnapshotService(
        this IUmbracoBuilder builder,
        IPublishedSnapshotService service)
    {
        builder.Services.AddUnique(service);
        return builder;
    }
}
