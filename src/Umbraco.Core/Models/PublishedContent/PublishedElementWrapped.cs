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

#pragma warning disable SA1402
/// <summary>
///     Provides an abstract base class for <c>IPublishedElement</c> implementations that
///     wrap and extend another <c>IPublishedElement</c>.
/// </summary>
/// <typeparam name="TElement">The type of element to wrap.</typeparam>
public abstract class PublishedElementWrapped<TElement> : IPublishedElement
    where TElement : IPublishedElement
{
    private readonly TElement _content;

    /// <summary>
    /// Initializes a new PublishedElementWrapped instance.
    /// </summary>
    /// <param name="content">The content to wrap.</param>
    protected PublishedElementWrapped(TElement content)
        => _content = content;

    /// <inheritdoc />
    public IPublishedContentType ContentType => _content.ContentType;

    /// <inheritdoc />
    public Guid Key => _content.Key;

    /// <inheritdoc />
    public virtual IEnumerable<IPublishedProperty> Properties => _content.Properties;

    /// <inheritdoc />
    public virtual IPublishedProperty? GetProperty(string alias) => _content.GetProperty(alias);

    /// <inheritdoc />
    public virtual int Id => _content.Id;

    /// <inheritdoc />
    public virtual string Name => _content.Name;

    /// <inheritdoc />
    public virtual int SortOrder => _content.SortOrder;

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
    public virtual bool IsDraft(string? culture = null) => _content.IsDraft(culture);

    /// <inheritdoc />
    public virtual bool IsPublished(string? culture = null) => _content.IsPublished(culture);

    /// <summary>
    ///     Gets the wrapped content.
    /// </summary>
    /// <returns>The wrapped content, that was passed as an argument to the constructor.</returns>
    public TElement Unwrap() => _content;
}
#pragma warning restore SA1402
