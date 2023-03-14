namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a strongly-typed published content.
/// </summary>
/// <remarks>
///     Every strongly-typed published content class should inherit from <c>PublishedContentModel</c>
///     (or inherit from a class that inherits from... etc.) so they are picked by the factory.
/// </remarks>
public abstract class PublishedContentModel : PublishedContentWrapped
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentModel" /> class with
    ///     an original <see cref="IPublishedContent" /> instance.
    /// </summary>
    /// <param name="content">The original content.</param>
    /// <param name="publishedValueFallback">the PublishedValueFallback</param>
    protected PublishedContentModel(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    {
    }
}
