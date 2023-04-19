using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

internal sealed class NoopRequestStartItemProvider : IRequestStartItemProvider
{
    /// <inheritdoc />
    public IPublishedContent? GetStartItem() => null;
}
