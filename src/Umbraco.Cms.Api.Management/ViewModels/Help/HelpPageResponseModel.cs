namespace Umbraco.Cms.Api.Management.ViewModels.Help;

/// <summary>
/// Represents a response model containing information for a help page in the management API.
/// </summary>
public class HelpPageResponseModel
{
    /// <summary>
    /// Gets or sets the name of the help page.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the help page.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the URL of the help page.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the category or classification of the help page.
    /// </summary>
    public string? Type { get; set; }
}
