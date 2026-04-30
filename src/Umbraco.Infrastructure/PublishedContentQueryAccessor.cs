using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Core;

/// <summary>
/// Provides access to the <see cref="IPublishedContentQuery"/> service, enabling queries against published content within Umbraco.
/// Typically used to retrieve or interact with published content data in a decoupled manner.
/// </summary>
public class PublishedContentQueryAccessor : IPublishedContentQueryAccessor
{
    private readonly IScopedServiceProvider _scopedServiceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishedContentQueryAccessor"/> class with the specified scoped service provider.
    /// </summary>
    /// <param name="scopedServiceProvider">
    /// The <see cref="IScopedServiceProvider"/> used to resolve services within the current scope.
    /// </param>
    public PublishedContentQueryAccessor(IScopedServiceProvider scopedServiceProvider) =>
        _scopedServiceProvider = scopedServiceProvider;

    /// <summary>
    /// Attempts to get the current <see cref="IPublishedContentQuery"/> instance.
    /// </summary>
    /// <param name="publishedContentQuery">When this method returns, contains the <see cref="IPublishedContentQuery"/> instance if available; otherwise, null.</param>
    /// <returns><c>true</c> if the <see cref="IPublishedContentQuery"/> instance was successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out IPublishedContentQuery publishedContentQuery)
    {
        publishedContentQuery = _scopedServiceProvider.ServiceProvider?.GetService<IPublishedContentQuery>();

        return publishedContentQuery is not null;
    }
}
