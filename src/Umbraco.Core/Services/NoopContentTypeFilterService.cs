using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// A no-op implementation of <see cref="IContentTypeFilterService"/>.
/// </summary>
/// <remarks>
/// This is registed with Umbraco as the default implementation of <see cref="IContentTypeFilterService"/> and will
/// apply no filters.
/// An implementor or package wishing to apply additional filtering to the content types retrieved from the database according
/// to the configured schema can replace this implementation with their own.
/// </remarks>
internal class NoopContentTypeFilterService : IContentTypeFilterService
{
    /// <inheritdoc/>
    public Task<IEnumerable<IContentTypeComposition>> FilterAllowedAtRootAsync(IEnumerable<IContentTypeComposition> contentTypes)
        => Task.FromResult(contentTypes);

    /// <inheritdoc/>
    public Task<IEnumerable<ContentTypeSort>> FilterAllowedChildrenAsync(IEnumerable<ContentTypeSort> contentTypes, Guid parentKey)
        => Task.FromResult(contentTypes);
}
