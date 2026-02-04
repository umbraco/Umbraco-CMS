namespace Umbraco.Cms.Core.Models.ContentPublishing;

/// <summary>
///     Represents the result of a content publishing operation.
/// </summary>
public sealed class ContentPublishingResult
{
    /// <summary>
    ///     Gets or initializes the content item that was published.
    /// </summary>
    public IPublishableContentBase? Content { get; init; }

    /// <summary>
    ///     Gets or sets the collection of property aliases that failed validation.
    /// </summary>
    public IEnumerable<string> InvalidPropertyAliases { get; set; } = [];
}
