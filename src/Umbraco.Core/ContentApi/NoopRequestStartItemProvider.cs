using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.ContentApi;

internal sealed class NoopRequestStartItemProvider : IRequestStartItemProvider
{
    /// <inheritdoc />
    public IPublishedContent? GetStartItem() => null;
}
