using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentNameProvider"/> that provides content names for the Delivery API.
/// </summary>
public sealed class ApiContentNameProvider : IApiContentNameProvider
{
    /// <inheritdoc />
    public string GetName(IPublishedContent content) => content.Name;
}
