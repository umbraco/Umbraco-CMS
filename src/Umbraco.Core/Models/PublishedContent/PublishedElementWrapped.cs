using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Provides an abstract base class for <c>IPublishedElement</c> implementations that
///     wrap and extend another <c>IPublishedElement</c>.
/// </summary>
[DebuggerDisplay("{Id}: {Name} ({ContentType?.Alias})")]
public abstract class PublishedElementWrapped : PublishedElementWrapped<IPublishedElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedElementWrapped" /> class
    ///     with an <c>IPublishedElement</c> instance to wrap.
    /// </summary>
    /// <param name="content">The content to wrap.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    protected PublishedElementWrapped(IPublishedElement content, IPublishedValueFallback publishedValueFallback)
        : base(content)
    {
    }
}
