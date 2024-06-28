
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;


namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco's NuCache
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco NuCache dependencies
    /// </summary>
    public static IUmbracoBuilder AddUmbracoHybridCache(this IUmbracoBuilder builder)
    {
        builder.Services.AddHybridCache();
        builder.Services.AddSingleton<IPublishedHybridCache, ContentCache>();
        builder.Services.AddSingleton<INuCacheContentRepository, NuCacheContentRepository>();
        builder.Services.AddSingleton<IContentCacheDataSerializerFactory>(s =>
        {
            IOptions<NuCacheSettings> options = s.GetRequiredService<IOptions<NuCacheSettings>>();
            switch (options.Value.NuCacheSerializerType)
            {
                case NuCacheSerializerType.JSON:
                    return new JsonContentNestedDataSerializerFactory();
                case NuCacheSerializerType.MessagePack:
                    return ActivatorUtilities.CreateInstance<MsgPackContentNestedDataSerializerFactory>(s);
                default:
                    throw new IndexOutOfRangeException();
            }
        });

        return builder;
    }

}
