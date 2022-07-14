using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core;

public class PublishedContentQueryAccessor : IPublishedContentQueryAccessor
{
    private readonly IScopedServiceProvider _scopedServiceProvider;

    [Obsolete("Please use alternative constructor")]
    public PublishedContentQueryAccessor(IServiceProvider serviceProvider) =>
        _scopedServiceProvider = serviceProvider.GetRequiredService<IScopedServiceProvider>();

    public PublishedContentQueryAccessor(IScopedServiceProvider scopedServiceProvider) =>
        _scopedServiceProvider = scopedServiceProvider;

    public bool TryGetValue([MaybeNullWhen(false)] out IPublishedContentQuery publishedContentQuery)
    {
        publishedContentQuery = _scopedServiceProvider.ServiceProvider?.GetService<IPublishedContentQuery>();

        return publishedContentQuery is not null;
    }
}
