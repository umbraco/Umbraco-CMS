namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides an abstract base class for <c>IPublishedElement</c> implementations that
///     wrap and extend another <c>IPublishedElement</c>.
/// </summary>
public abstract class PublishedElementWrapped : IPublishedElement
{
    private readonly IPublishedElement _content;
    private readonly IPublishedValueFallback _publishedValueFallback;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedElementWrapped" /> class
    ///     with an <c>IPublishedElement</c> instance to wrap.
    /// </summary>
    /// <param name="content">The content to wrap.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    protected PublishedElementWrapped(IPublishedElement content, IPublishedValueFallback publishedValueFallback)
    {
        _content = content;
        _publishedValueFallback = publishedValueFallback;
    }

    /// <inheritdoc />
    public IPublishedContentType ContentType => _content.ContentType;

    /// <inheritdoc />
    public Guid Key => _content.Key;

    /// <inheritdoc />
    public IEnumerable<IPublishedProperty> Properties => _content.Properties;

    /// <inheritdoc />
    public IPublishedProperty? GetProperty(string alias) => _content.GetProperty(alias);

    /// <summary>
    ///     Gets the wrapped content.
    /// </summary>
    /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
    public IPublishedElement Unwrap() => _content;
}
