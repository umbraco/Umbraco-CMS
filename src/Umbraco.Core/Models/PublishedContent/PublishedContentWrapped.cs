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
public abstract class PublishedContentWrapped : IPublishedContent
{
    private readonly IPublishedContent _content;
    private readonly IPublishedValueFallback _publishedValueFallback;

    /// <summary>
    ///     Initialize a new instance of the <see cref="PublishedContentWrapped" /> class
    ///     with an <c>IPublishedContent</c> instance to wrap.
    /// </summary>
    /// <param name="content">The content to wrap.</param>
    /// <param name="publishedValueFallback">The published value fallback.</param>
    protected PublishedContentWrapped(IPublishedContent content, IPublishedValueFallback publishedValueFallback)
    {
        _content = content;
        _publishedValueFallback = publishedValueFallback;
    }

    /// <inheritdoc />
    public virtual IPublishedContentType ContentType => _content.ContentType;

    /// <inheritdoc />
    public Guid Key => _content.Key;

    #region PublishedContent

    /// <inheritdoc />
    public virtual int Id => _content.Id;

    #endregion

    /// <summary>
    ///     Gets the wrapped content.
    /// </summary>
    /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
    public IPublishedContent Unwrap() => _content;

    /// <inheritdoc />
    public virtual string? Name => _content.Name;

    /// <inheritdoc />
    public virtual string? UrlSegment => _content.UrlSegment;

    /// <inheritdoc />
    public virtual int SortOrder => _content.SortOrder;

    /// <inheritdoc />
    public virtual int Level => _content.Level;

    /// <inheritdoc />
    public virtual string Path => _content.Path;

    /// <inheritdoc />
    public virtual int? TemplateId => _content.TemplateId;

    /// <inheritdoc />
    public virtual int CreatorId => _content.CreatorId;

    /// <inheritdoc />
    public virtual DateTime CreateDate => _content.CreateDate;

    /// <inheritdoc />
    public virtual int WriterId => _content.WriterId;

    /// <inheritdoc />
    public virtual DateTime UpdateDate => _content.UpdateDate;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => _content.Cultures;

    /// <inheritdoc />
    public virtual PublishedItemType ItemType => _content.ItemType;

    /// <inheritdoc />
    public virtual IPublishedContent? Parent => _content.Parent;

    /// <inheritdoc />
    public virtual bool IsDraft(string? culture = null) => _content.IsDraft(culture);

    /// <inheritdoc />
    public virtual bool IsPublished(string? culture = null) => _content.IsPublished(culture);

    /// <inheritdoc />
    public virtual IEnumerable<IPublishedContent>? Children => _content.Children;

    /// <inheritdoc />
    public virtual IEnumerable<IPublishedContent>? ChildrenForAllCultures => _content.ChildrenForAllCultures;

    /// <inheritdoc cref="IPublishedElement.Properties" />
    public virtual IEnumerable<IPublishedProperty> Properties => _content.Properties;

    /// <inheritdoc cref="IPublishedElement.GetProperty(string)" />
    public virtual IPublishedProperty? GetProperty(string alias) => _content.GetProperty(alias);
}
