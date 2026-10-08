using System.Diagnostics;

namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Provide an abstract base class for publishable content implementations (like <c>IPublishedContent</c> and <c>IPublishedElement</c> implementations).
/// </summary>
[DebuggerDisplay("Content Id: {Id}")]
public abstract class PublishableContentBase
{
    public abstract IPublishedContentType ContentType { get; }

    /// <inheritdoc />
    public abstract Guid Key { get; }

    /// <inheritdoc />
    public abstract int Id { get; }

    /// <inheritdoc />
    public abstract int SortOrder { get; }

    /// <inheritdoc />
    public abstract int CreatorId { get; }

    /// <inheritdoc />
    public abstract DateTime CreateDate { get; }

    /// <inheritdoc />
    public abstract int WriterId { get; }

    /// <inheritdoc />
    public abstract DateTime UpdateDate { get; }

    /// <inheritdoc />
    public abstract IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

    /// <inheritdoc />
    public abstract PublishedItemType ItemType { get; }

    /// <inheritdoc />
    public abstract bool IsDraft(string? culture = null);

    /// <inheritdoc />
    public abstract bool IsPublished(string? culture = null);

    /// <inheritdoc cref="IPublishedElement.Properties"/>
    public abstract IEnumerable<IPublishedProperty> Properties { get; }

    /// <inheritdoc cref="IPublishedElement.GetProperty(string)"/>
    public abstract IPublishedProperty? GetProperty(string alias);
}
