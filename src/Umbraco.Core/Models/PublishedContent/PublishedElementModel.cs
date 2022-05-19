namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <inheritdoc />
/// <summary>
///     Represents a strongly-typed published element.
/// </summary>
/// <remarks>
///     Every strongly-typed property set class should inherit from <c>PublishedElementModel</c>
///     (or inherit from a class that inherits from... etc.) so they are picked by the factory.
/// </remarks>
public abstract class PublishedElementModel : PublishedElementWrapped
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedElementModel" /> class with
    ///     an original <see cref="IPublishedElement" /> instance.
    /// </summary>
    /// <param name="content">The original content.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    protected PublishedElementModel(IPublishedElement content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }
}
