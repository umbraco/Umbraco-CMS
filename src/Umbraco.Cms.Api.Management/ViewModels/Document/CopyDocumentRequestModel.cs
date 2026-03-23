namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the data required to copy a document via the management API, including target location and copy options.
/// </summary>
public class CopyDocumentRequestModel
{
    /// <summary>
    /// Gets or sets the target location, specified by ID, where the document will be copied.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the copied document should be related to the original document.
    /// </summary>
    public bool RelateToOriginal { get; set; }

    /// <summary>Gets or sets a value indicating whether to include descendant documents when copying.</summary>
    public bool IncludeDescendants { get; set; }
}
