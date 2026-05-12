namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents a published element.
/// </summary>
public interface IPublishedElement
{
    #region ContentType

    /// <summary>
    ///     Gets the content type.
    /// </summary>
    IPublishedContentType ContentType { get; }

    #endregion

    #region PublishedElement

    /// <summary>
    ///     Gets the unique key of the published element.
    /// </summary>
    Guid Key { get; }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the properties of the element.
    /// </summary>
    /// <remarks>
    ///     Contains one <c>IPublishedProperty</c> for each property defined for the content type, including
    ///     inherited properties. Some properties may have no value.
    /// </remarks>
    IEnumerable<IPublishedProperty> Properties { get; }

    /// <summary>
    ///     Gets a property identified by its alias.
    /// </summary>
    /// <param name="alias">The property alias.</param>
    /// <returns>The property identified by the alias.</returns>
    /// <remarks>
    ///     <para>If the content type has no property with that alias, including inherited properties, returns <c>null</c>,</para>
    ///     <para>otherwise return a property -- that may have no value (ie <c>HasValue</c> is <c>false</c>).</para>
    ///     <para>The alias is case-insensitive.</para>
    /// </remarks>
    IPublishedProperty? GetProperty(string alias);

    #endregion

    /// <summary>
    ///     Gets the unique identifier of the content item.
    /// </summary>
    int Id { get; }

    /// <summary>
    ///     Gets the name of the content item for the current culture.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the sort order of the content item.
    /// </summary>
    int SortOrder { get; }

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
    ///     // TODO?
    /// </remarks>
    IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; }

    /// <summary>
    ///     Gets the type of the content item (document, media...).
    /// </summary>
    PublishedItemType ItemType { get; }

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
}
