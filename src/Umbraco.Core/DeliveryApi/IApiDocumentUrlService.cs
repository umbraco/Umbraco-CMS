namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that retrieves document information by URL route for the Delivery API.
/// </summary>
public interface IApiDocumentUrlService
{
    /// <summary>
    ///     Gets the document key (unique identifier) for the specified route.
    /// </summary>
    /// <param name="route">The URL route to look up.</param>
    /// <param name="culture">The culture to use for the lookup, or <c>null</c> for the default culture.</param>
    /// <param name="preview">Whether to include unpublished preview content in the lookup.</param>
    /// <returns>The document key if found, or <c>null</c> if no document exists at the specified route.</returns>
    Guid? GetDocumentKeyByRoute(string route, string? culture, bool preview);
}
