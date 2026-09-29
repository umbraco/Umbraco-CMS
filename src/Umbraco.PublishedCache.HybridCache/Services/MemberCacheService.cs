using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

/// <summary>
/// Implements a service for mapping member entities to published members.
/// </summary>
internal sealed class MemberCacheService : IMemberCacheService
{
    private readonly IPublishedContentFactory _publishedContentFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberCacheService"/> class.
    /// </summary>
    /// <param name="publishedContentFactory">The published content factory.</param>
    public MemberCacheService(IPublishedContentFactory publishedContentFactory)
        => _publishedContentFactory = publishedContentFactory;

    /// <inheritdoc/>
    public async Task<IPublishedMember?> Get(IMember member) => member is null ? null : _publishedContentFactory.ToPublishedMember(member);
}
