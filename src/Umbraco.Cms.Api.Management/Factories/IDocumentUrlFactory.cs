using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory interface for generating URLs for documents.
/// </summary>
public interface IDocumentUrlFactory
{
    /// <summary>
    /// Creates URLs asynchronously for the given content.
    /// </summary>
    /// <param name="content">The content to create URLs for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of DocumentUrlInfo.</returns>
    Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content);

    /// <summary>
    /// Asynchronously creates URL sets for the specified content items.
    /// </summary>
    /// <param name="contentItems">The content items for which to generate URL sets.</param>
    /// <returns>A task representing the asynchronous operation, with a result containing an enumerable of <see cref="DocumentUrlInfoResponseModel"/> representing the URL sets.</returns>
    Task<IEnumerable<DocumentUrlInfoResponseModel>> CreateUrlSetsAsync(IEnumerable<IContent> contentItems);

    /// <summary>
    /// Asynchronously retrieves preview URL information for the specified content item.
    /// </summary>
    /// <param name="content">The content item for which to generate the preview URL.</param>
    /// <param name="providerAlias">The alias of the URL provider to use.</param>
    /// <param name="culture">An optional culture identifier for the URL, or <c>null</c> to use the default.</param>
    /// <param name="segment">An optional segment for the URL, or <c>null</c> if not applicable.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the <see cref="DocumentUrlInfo"/> for the preview URL, or <c>null</c> if unavailable.</returns>
    Task<DocumentUrlInfo?> GetPreviewUrlAsync(IContent content, string providerAlias, string? culture, string? segment);
}
