using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

// we cannot implement strongly-typed content by inheriting from some sort
// of "master content" because that master content depends on the actual content cache
// that is being used. It can be an XmlPublishedContent with the XmlPublishedCache,
// or just anything else.
//
// So we implement strongly-typed content by encapsulating whatever content is
// returned by the content cache, and providing extra properties (mostly) or
// methods or whatever. This class provides the base for such encapsulation.
//

/// <summary>
///     Provides an abstract base class for <c>IPublishedContent</c> implementations that
///     wrap and extend another <c>IPublishedContent</c>.
/// </summary>
[DebuggerDisplay("{Id}: {Name} ({ContentType?.Alias})")]
public abstract class PublishedContentWrapped : PublishedElementWrapped<IPublishedContent>, IPublishedContent
{
    private readonly IPublishedContent _content;

    /// <summary>
    ///     Initialize a new instance of the <see cref="PublishedContentWrapped" /> class
    ///     with an <c>IPublishedContent</c> instance to wrap.
    /// </summary>
    /// <param name="content">The content to wrap.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    protected PublishedContentWrapped(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
        : base(content)
        => _content = content;

    /// <inheritdoc />
    public virtual string? UrlSegment => _content.UrlSegment;

    /// <inheritdoc />
    public virtual int Level => _content.Level;

    /// <inheritdoc />
    public virtual string Path => _content.Path;

    /// <inheritdoc />
    public virtual int? TemplateId => _content.TemplateId;
}
