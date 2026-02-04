using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestStartItemProvider"/> that never returns a start item.
/// </summary>
internal sealed class NoopRequestStartItemProvider : IRequestStartItemProvider
{
    /// <inheritdoc />
    public IPublishedContent? GetStartItem() => null;

    /// <inheritdoc />
    public string? RequestedStartItem() => null;
}
