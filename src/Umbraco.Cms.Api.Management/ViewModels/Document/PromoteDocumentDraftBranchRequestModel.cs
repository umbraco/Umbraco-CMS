namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API request model for promoting a document draft branch.
/// </summary>
public class PromoteDocumentDraftBranchRequestModel
{
    /// <summary>
    /// Gets or sets the cultures to promote. For culture-variant content, must be non-empty and every entry
    /// must exist on the branch's own content. For invariant content, must be empty.
    /// </summary>
    public ISet<string> Cultures { get; set; } = new HashSet<string>();
}
