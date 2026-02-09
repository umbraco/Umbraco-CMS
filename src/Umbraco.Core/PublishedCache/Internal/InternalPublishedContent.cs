using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PublishedCache.Internal;

// TODO: Only used in unit tests, needs to be moved to test project

/// <summary>
/// Provides an internal implementation of <see cref="IPublishedContent"/> for testing purposes.
/// </summary>
/// <remarks>
/// This class is intended for unit testing only and should not be used in production code.
/// It should be moved to a test project in a future refactoring.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InternalPublishedContent : IPublishedContent
{
    private Dictionary<string, PublishedCultureInfo>? _cultures;

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalPublishedContent"/> class.
    /// </summary>
    /// <param name="contentType">The published content type.</param>
    public InternalPublishedContent(IPublishedContentType contentType)
    {
        // initialize boring stuff
        TemplateId = 0;
        WriterId = CreatorId = 0;
        CreateDate = UpdateDate = DateTime.UtcNow;
        Version = Guid.Empty;
        Path = string.Empty;
        ContentType = contentType;
        Properties = Enumerable.Empty<IPublishedProperty>();
        Name = string.Empty;
    }

    /// <summary>
    /// Gets or sets the version identifier.
    /// </summary>
    public Guid Version { get; set; }

    /// <summary>
    /// Gets or sets the parent content identifier.
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// Gets or sets the collection of child content identifiers.
    /// </summary>
    public IEnumerable<int> ChildIds { get; set; } = Enumerable.Empty<int>();

    /// <inheritdoc />
    public int Id { get; set; }

    /// <summary>
    /// Gets the value of a property by its alias.
    /// </summary>
    /// <param name="alias">The property alias.</param>
    /// <returns>The property value, or <c>null</c> if the property does not exist or has no value.</returns>
    public object? this[string alias]
    {
        get
        {
            IPublishedProperty? property = GetProperty(alias);
            return property == null || property.HasValue() == false ? null : property.GetValue();
        }
    }

    /// <inheritdoc />
    public Guid Key { get; set; }

    /// <inheritdoc />
    public int? TemplateId { get; set; }

    /// <inheritdoc />
    public int SortOrder { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures => _cultures ??= GetCultures();

    /// <inheritdoc />
    public string? UrlSegment { get; set; }

    /// <inheritdoc />
    public int WriterId { get; set; }

    /// <inheritdoc />
    public int CreatorId { get; set; }

    /// <inheritdoc />
    public string Path { get; set; }

    /// <inheritdoc />
    public DateTime CreateDate { get; set; }

    /// <inheritdoc />
    public DateTime UpdateDate { get; set; }

    /// <inheritdoc />
    public int Level { get; set; }

    /// <inheritdoc />
    public PublishedItemType ItemType => PublishedItemType.Content;

    /// <inheritdoc />
    [Obsolete("Please use TryGetParentKey() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
    public IPublishedContent? Parent => this.Parent<IPublishedContent>(StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>(), StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>());

    /// <inheritdoc />
    public bool IsDraft(string? culture = null) => false;

    /// <inheritdoc />
    public bool IsPublished(string? culture = null) => true;

    /// <inheritdoc />
    [Obsolete("Please use TryGetChildrenKeys() on IDocumentNavigationQueryService or IMediaNavigationQueryService instead. Scheduled for removal in V16.")]
    public IEnumerable<IPublishedContent> Children => this.Children(
        StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>(),
        StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>());

    /// <inheritdoc />
    public IEnumerable<IPublishedContent> ChildrenForAllCultures => Children;

    /// <inheritdoc />
    public IPublishedContentType ContentType { get; set; }

    /// <inheritdoc />
    public IEnumerable<IPublishedProperty> Properties { get; set; }

    /// <inheritdoc />
    public IPublishedProperty? GetProperty(string alias) =>
        Properties?.FirstOrDefault(p => p.Alias.InvariantEquals(alias));

    /// <summary>
    /// Gets a property by its alias, optionally recursing up the content tree.
    /// </summary>
    /// <param name="alias">The property alias.</param>
    /// <param name="recurse">A value indicating whether to recurse up the content tree if the property is not found or has no value.</param>
    /// <returns>The property, or <c>null</c> if not found.</returns>
    public IPublishedProperty? GetProperty(string alias, bool recurse)
    {
        IPublishedProperty? property = GetProperty(alias);
        if (recurse == false)
        {
            return property;
        }

        IPublishedContent? content = this;
        while (content != null && (property == null || property.HasValue() == false))
        {
            content = content.Parent<IPublishedContent>(StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>(), StaticServiceProvider.Instance.GetRequiredService<IPublishedContentStatusFilteringService>());
            property = content?.GetProperty(alias);
        }

        return property;
    }

    /// <summary>
    /// Gets the culture information dictionary for this content.
    /// </summary>
    /// <returns>A dictionary mapping culture codes to <see cref="PublishedCultureInfo"/> instances.</returns>
    private Dictionary<string, PublishedCultureInfo> GetCultures() => new()
    {
        { string.Empty, new PublishedCultureInfo(string.Empty, Name, UrlSegment, UpdateDate) },
    };
}
