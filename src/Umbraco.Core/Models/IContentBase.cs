using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides a base class for content items.
/// </summary>
/// <remarks>
///     <para>Content items are documents, medias and members.</para>
///     <para>Content items have a content type, and properties.</para>
/// </remarks>
public interface IContentBase : IUmbracoEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Integer Id of the default ContentType
    /// </summary>
    int ContentTypeId { get; }

    /// <summary>
    ///     Gets the content type of this content.
    /// </summary>
    ISimpleContentType ContentType { get; }

    /// <summary>
    ///     Gets the identifier of the writer.
    /// </summary>
    int WriterId { get; set; }

    /// <summary>
    ///     Gets the version identifier.
    /// </summary>
    int VersionId { get; set; }

    /// <summary>
    ///     Gets culture infos of the content item.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Because a dictionary key cannot be <c>null</c> this cannot contain the invariant
    ///         culture name, which must be get or set via the <see cref="TreeEntityBase.Name" /> property.
    ///     </para>
    /// </remarks>
    ContentCultureInfosCollection? CultureInfos { get; set; }

    /// <summary>
    ///     Gets the available cultures.
    /// </summary>
    /// <remarks>
    ///     <para>Cannot contain the invariant culture, which is always available.</para>
    /// </remarks>
    IEnumerable<string> AvailableCultures { get; }

    /// <summary>
    ///     List of properties, which make up all the data available for this Content object
    /// </summary>
    /// <remarks>Properties are loaded as part of the Content object graph</remarks>
    IPropertyCollection Properties { get; set; }

    /// <summary>
    ///     Sets the name of the content item for a specified culture.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <paramref name="culture" /> is null, sets the invariant
    ///         culture name, which sets the <see cref="TreeEntityBase.Name" /> property.
    ///     </para>
    ///     <para>
    ///         When <paramref name="culture" /> is not null, throws if the content
    ///         type does not vary by culture.
    ///     </para>
    /// </remarks>
    void SetCultureName(string? value, string? culture);

    /// <summary>
    ///     Gets the name of the content item for a specified language.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <paramref name="culture" /> is null, gets the invariant
    ///         culture name, which is the value of the <see cref="TreeEntityBase.Name" /> property.
    ///     </para>
    ///     <para>
    ///         When <paramref name="culture" /> is not null, and the content type
    ///         does not vary by culture, returns null.
    ///     </para>
    /// </remarks>
    string? GetCultureName(string? culture);

    /// <summary>
    ///     Gets a value indicating whether a given culture is available.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A culture becomes available whenever the content name for this culture is
    ///         non-null, and it becomes unavailable whenever the content name is null.
    ///     </para>
    ///     <para>
    ///         Returns <c>false</c> for the invariant culture, in order to be consistent
    ///         with <seealso cref="AvailableCultures" />, even though the invariant culture is
    ///         always available.
    ///     </para>
    ///     <para>Does not support the '*' wildcard (returns false).</para>
    /// </remarks>
    bool IsCultureAvailable(string culture);

    /// <summary>
    ///     Gets the date a culture was updated.
    /// </summary>
    /// <remarks>
    ///     <para>When <paramref name="culture" /> is <c>null</c>, returns <c>null</c>.</para>
    ///     <para>If the specified culture is not available, returns <c>null</c>.</para>
    /// </remarks>
    DateTime? GetUpdateDate(string culture);

    /// <summary>
    ///     Gets a value indicating whether the content entity has a property with the supplied alias.
    /// </summary>
    /// <remarks>
    ///     Indicates that the content entity has a property with the supplied alias, but
    ///     not necessarily that the content has a value for that property. Could be missing.
    /// </remarks>
    bool HasProperty(string propertyTypeAlias);

    /// <summary>
    ///     Gets the value of a Property
    /// </summary>
    /// <remarks>Values 'null' and 'empty' are equivalent for culture and segment.</remarks>
    object? GetValue(string propertyTypeAlias, string? culture = null, string? segment = null, bool published = false);

    /// <summary>
    ///     Gets the typed value of a Property
    /// </summary>
    /// <remarks>Values 'null' and 'empty' are equivalent for culture and segment.</remarks>
    TValue? GetValue<TValue>(string propertyTypeAlias, string? culture = null, string? segment = null, bool published = false);

    /// <summary>
    ///     Sets the (edited) value of a Property
    /// </summary>
    /// <remarks>Values 'null' and 'empty' are equivalent for culture and segment.</remarks>
    void SetValue(string propertyTypeAlias, object? value, string? culture = null, string? segment = null);
}
