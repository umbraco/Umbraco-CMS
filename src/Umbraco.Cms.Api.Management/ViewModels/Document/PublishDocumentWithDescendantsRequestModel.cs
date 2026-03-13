namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Request model for publishing a document and all of its descendants.
/// </summary>
public class PublishDocumentWithDescendantsRequestModel
{
    /// <summary>
    /// Gets or sets a value indicating whether to include unpublished descendant documents when publishing.
    /// </summary>
    public bool IncludeUnpublishedDescendants { get; set; }

    /// <summary>
    /// Gets or sets the cultures for which the document and its descendants should be published.
    /// </summary>
    public required IEnumerable<string> Cultures { get; set; }
}
