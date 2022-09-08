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
    // TODO: IPublishedContent properties colliding with models
    // we need to find a way to remove as much clutter as possible from IPublishedContent,
    // since this is preventing someone from creating a property named 'Path' and have it
    // in a model, for instance. we could move them all under one unique property eg
    // Infos, so we would do .Infos.SortOrder - just an idea - not going to do it in v8

    /// <summary>
    ///     Gets the unique identifier of the content item.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the name of the content item for the current culture.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the URL segment of the content item for the current culture.
    /// </summary>
    string? UrlSegment { get; }

    /// <summary>
    ///     Gets the sort order of the content item.
    /// </summary>
    int SortOrder { get; }

    /// <summary>
    ///     Gets the tree level of the content item.
    /// </summary>
    int Level { get; }

    /// <summary>
    ///     Gets the tree path of the content item.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Gets the identifier of the template to use to render the content item.
    /// </summary>
    int? TemplateId { get; }

    /// <summary>
    ///     Gets the identifier of the user who created the content item.
    /// </summary>
    int CreatorId { get; }

    /// <summary>
    ///     Gets the date the content item was created.
    /// </summary>
    DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the identifier of the user who last updated the content item.
    /// </summary>
    int WriterId { get; }

    /// <summary>
    ///     Gets the date the content item was last updated.
    /// </summary>
    /// <remarks>
    ///     <para>For published content items, this is also the date the item was published.</para>
    ///     <para>
    ///         This date is always global to the content item, see CultureDate() for the
    ///         date each culture was published.
    ///     </para>
    /// </remarks>
    DateTime UpdateDate { get; }

    /// <summary>
    ///     Gets available culture infos.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Contains only those culture that are available. For a published content, these are
    ///         the cultures that are published. For a draft content, those that are 'available' ie
    ///         have a non-empty content name.
    ///     </para>
    ///     <para>Does not contain the invariant culture.</para>
    ///     // fixme?
    /// </remarks>
    IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

    /// <summary>
    ///     Gets the type of the content item (document, media...).
    /// </summary>
    PublishedItemType ItemType { get; }

    /// <summary>
    ///     Gets the parent of the content item.
    /// </summary>
    /// <remarks>The parent of root content is <c>null</c>.</remarks>
    IPublishedContent? Parent { get; }

    /// <summary>
    ///     Gets a value indicating whether the content is draft.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A content is draft when it is the unpublished version of a content, which may
    ///         have a published version, or not.
    ///     </para>
    ///     <para>
    ///         When retrieving documents from cache in non-preview mode, IsDraft is always false,
    ///         as only published documents are returned. When retrieving in preview mode, IsDraft can
    ///         either be true (document is not published, or has been edited, and what is returned
    ///         is the edited version) or false (document is published, and has not been edited, and
    ///         what is returned is the published version).
    ///     </para>
    /// </remarks>
    bool IsDraft(string? culture = null);

    /// <summary>
    ///     Gets a value indicating whether the content is published.
    /// </summary>
    /// <remarks>
    ///     <para>A content is published when it has a published version.</para>
    ///     <para>
    ///         When retrieving documents from cache in non-preview mode, IsPublished is always
    ///         true, as only published documents are returned. When retrieving in draft mode, IsPublished
    ///         can either be true (document has a published version) or false (document has no
    ///         published version).
    ///     </para>
    ///     <para>
    ///         It is therefore possible for both IsDraft and IsPublished to be true at the same
    ///         time, meaning that the content is the draft version, and a published version exists.
    ///     </para>
    /// </remarks>
    bool IsPublished(string? culture = null);

    /// <summary>
    ///     Gets the children of the content item that are available for the current culture.
    /// </summary>
    IEnumerable<IPublishedContent>? Children { get; }

    /// <summary>
    ///     Gets all the children of the content item, regardless of whether they are available for the current culture.
    /// </summary>
    IEnumerable<IPublishedContent>? ChildrenForAllCultures { get; }
}
