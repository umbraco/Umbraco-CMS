namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>
///     Represents a published content item.
/// </summary>
/// <remarks>
///     <para>Can be a published document, media or member.</para>
/// </remarks>
public interface IPublishedContent : IPublishedElement
{
    /// <summary>
    ///     Gets the URL segment of the content item for the current culture.
    /// </summary>
    string? UrlSegment { get; }

    /// <summary>
    ///     Gets the identifier of the template to use to render the content item.
    /// </summary>
    int? TemplateId { get; }

    /// <summary>
    ///     Gets the tree level of the content item.
    /// </summary>
    int Level { get; }

    /// <summary>
    ///     Gets the tree path of the content item.
    /// </summary>
    string Path { get; }
}
