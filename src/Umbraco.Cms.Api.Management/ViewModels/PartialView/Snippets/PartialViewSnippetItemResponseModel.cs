namespace Umbraco.Cms.Api.Management.ViewModels.PartialView.Snippets;

/// <summary>
/// Represents the data returned for a single partial view snippet item in the API response.
/// </summary>
public class PartialViewSnippetItemResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the partial view snippet item.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the partial view snippet.
    /// </summary>
    public required string Name { get; set; }
}
