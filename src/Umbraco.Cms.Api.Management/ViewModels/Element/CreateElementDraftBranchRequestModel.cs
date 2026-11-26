namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API request model for creating a new named draft branch of an element.
/// </summary>
public class CreateElementDraftBranchRequestModel
{
    /// <summary>
    /// Gets or sets the editor-facing name of the new draft branch (e.g. "Black Friday").
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
