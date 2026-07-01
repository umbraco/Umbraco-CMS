namespace Umbraco.Cms.Api.Management.ViewModels.Content;

/// <summary>
/// Provides base information about a content item's URL.
/// </summary>
public abstract class ContentUrlInfoBase
{
    /// <summary>
    /// Gets or sets the culture identifier (e.g., language or locale) associated with the content URL.
    /// </summary>
    public required string? Culture { get; init; }

    /// <summary>
    /// Gets or sets the URL associated with the content.
    /// </summary>
    public required string? Url { get; init; }
}
