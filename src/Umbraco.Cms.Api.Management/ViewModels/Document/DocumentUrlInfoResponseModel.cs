namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Response model containing URL information for a document.
/// </summary>
public sealed class DocumentUrlInfoResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlInfoResponseModel"/> class with the specified document identifier and URL information.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <param name="urlInfos">A collection of URL information related to the document.</param>
    public DocumentUrlInfoResponseModel(Guid id, IEnumerable<DocumentUrlInfo> urlInfos)
    {
        Id = id;
        UrlInfos = urlInfos;
    }

    /// <summary>
    /// Gets the unique identifier of the document URL info.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the collection of URL information associated with the document.
    /// </summary>
    public IEnumerable<DocumentUrlInfo> UrlInfos { get; }
}
