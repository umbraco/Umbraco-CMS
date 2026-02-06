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
    ///     Gets the parent of the content item.
    /// </summary>
    /// <remarks>The parent of root content is <c>null</c>.</remarks>
    [Obsolete("Please use either the IPublishedContent.Parent<>() extension method in the Umbraco.Extensions namespace, or IDocumentNavigationQueryService if you only need keys. Scheduled for removal in Umbraco 18.")]
    IPublishedContent? Parent { get; }

    /// <summary>
    ///     Gets the children of the content item that are available for the current culture.
    /// </summary>
    [Obsolete("Please use either the IPublishedContent.Children() extension method in the Umbraco.Extensions namespace, or IDocumentNavigationQueryService if you only need keys. Scheduled for removal in Umbraco 18.")]
    IEnumerable<IPublishedContent> Children { get; }

    /// <summary>
    ///     Gets the tree level of the content item.
    /// </summary>
    int Level { get; }

    /// <summary>
    ///     Gets the tree path of the content item.
    /// </summary>
    string Path { get; }
}
