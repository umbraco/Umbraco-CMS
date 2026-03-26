namespace Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;

/// <summary>
/// Represents the API response model containing information about a partial view snippet in Umbraco.
/// </summary>
public class PartialViewSnippetResponseModel : PartialViewSnippetItemResponseModel
{
    /// <summary>
    /// Gets or sets the content of the partial view snippet.
    /// </summary>
    public required string Content { get; set; }
}
