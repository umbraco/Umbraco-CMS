using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Defines the base for a ContentType with properties that
///     are shared between ContentTypes and MediaTypes.
/// </summary>
public interface IContentTypeBase : IUmbracoEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or Sets the Alias of the ContentType
    /// </summary>
    string Alias { get; set; }

    /// <summary>
    ///     Gets or Sets the Description for the ContentType
    /// </summary>
    string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the icon for the content type. The value is a CSS class name representing
    ///     the icon (eg. <c>icon-home</c>) along with an optional CSS class name representing the
    ///     color (eg. <c>icon-blue</c>). Put together, the value for this scenario would be
    ///     <c>icon-home color-blue</c>.
    ///     If a class name for the color isn't specified, the icon color will default to black.
    /// </summary>
    string? Icon { get; set; }

    /// <summary>
    ///     Gets or Sets the Thumbnail for the ContentType
    /// </summary>
    string? Thumbnail { get; set; }

    /// <summary>
    ///     Gets or Sets a boolean indicating whether this ContentType is allowed at the root
    /// </summary>
    bool AllowedAsRoot { get; set; }

    /// <summary>
    ///     Gets or Sets a boolean indicating whether this ContentType is a Container
    /// </summary>
    /// <remarks>
    ///     ContentType Containers doesn't show children in the tree, but rather in grid-type view.
    /// </remarks>
    bool IsContainer { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this content type is for an element.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default a content type is for a true media, member or document, but
    ///         it can also be for an element, ie a subset that can for instance be used in
    ///         nested content.
    ///     </para>
    /// </remarks>
    bool IsElement { get; set; }

    /// <summary>
    ///     Gets or sets the content variation of the content type.
    /// </summary>
    ContentVariation Variations { get; set; }

    /// <summary>
    ///     Gets or Sets a list of integer Ids of the ContentTypes allowed under the ContentType
    /// </summary>
    IEnumerable<ContentTypeSort>? AllowedContentTypes { get; set; }

    /// <summary>
    ///     Gets or sets the local property groups.
    /// </summary>
    PropertyGroupCollection PropertyGroups { get; set; }

    /// <summary>
    ///     Gets all local property types all local property groups or ungrouped.
    /// </summary>
    IEnumerable<IPropertyType> PropertyTypes { get; }

    /// <summary>
    ///     Gets or sets the local property types that do not belong to a group.
    /// </summary>
    IEnumerable<IPropertyType> NoGroupPropertyTypes { get; set; }

    /// <summary>
    ///     Validates that a combination of culture and segment is valid for the content type.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="wildcards">A value indicating whether wildcard are supported.</param>
    /// <returns>True if the combination is valid; otherwise false.</returns>
    /// <remarks>
    ///     <para>
    ///         The combination must match the content type variation exactly. For instance, if the content type varies by
    ///         culture,
    ///         then an invariant culture would be invalid.
    ///     </para>
    /// </remarks>
    bool SupportsVariation(string culture, string segment, bool wildcards = false);

    /// <summary>
    ///     Validates that a combination of culture and segment is valid for the content type properties.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <param name="segment">The segment.</param>
    /// <param name="wildcards">A value indicating whether wildcard are supported.</param>
    /// <returns>True if the combination is valid; otherwise false.</returns>
    /// <remarks>
    ///     <para>
    ///         The combination must be valid for properties of the content type. For instance, if the content type varies by
    ///         culture,
    ///         then an invariant culture is valid, because some properties may be invariant. On the other hand, if the content
    ///         type is invariant,
    ///         then a variant culture is invalid, because no property could possibly vary by culture.
    ///     </para>
    /// </remarks>
    bool SupportsPropertyVariation(string culture, string segment, bool wildcards = false);

    /// <summary>
    ///     Removes a PropertyType from the current ContentType
    /// </summary>
    /// <param name="alias">Alias of the <see cref="IPropertyType" /> to remove</param>
    void RemovePropertyType(string alias);

    /// <summary>
    ///     Removes a property group from the current content type.
    /// </summary>
    /// <param name="alias">Alias of the <see cref="PropertyGroup" /> to remove</param>
    void RemovePropertyGroup(string alias);

    /// <summary>
    ///     Checks whether a PropertyType with a given alias already exists
    /// </summary>
    /// <param name="alias">Alias of the PropertyType</param>
    /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
    bool PropertyTypeExists(string? alias);

    /// <summary>
    ///     Adds the property type to the specified property group (creates a new group if not found and a name is specified).
    /// </summary>
    /// <param name="propertyType">The property type to add.</param>
    /// <param name="propertyGroupAlias">The alias of the property group to add the property type to.</param>
    /// <param name="propertyGroupName">The name of the property group to create when not found.</param>
    /// <returns>
    ///     Returns <c>true</c> if the property type was added; otherwise, <c>false</c>.
    /// </returns>
    bool AddPropertyType(IPropertyType propertyType, string propertyGroupAlias, string? propertyGroupName = null);

    /// <summary>
    ///     Adds a PropertyType, which does not belong to a PropertyGroup.
    /// </summary>
    /// <param name="propertyType"><see cref="IPropertyType" /> to add</param>
    /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
    bool AddPropertyType(IPropertyType propertyType);

    /// <summary>
    ///     Adds a property group with the specified <paramref name="name" /> and <paramref name="alias" />.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="name">Name of the group.</param>
    /// <returns>
    ///     Returns <c>true</c> if a property group with specified <paramref name="alias" /> was added; otherwise, <c>false</c>
    ///     .
    /// </returns>
    /// <remarks>
    ///     This method will also check if a group already exists with the same alias.
    /// </remarks>
    bool AddPropertyGroup(string alias, string name);

    /// <summary>
    ///     Moves a PropertyType to a specified PropertyGroup
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to move</param>
    /// <param name="propertyGroupAlias">Alias of the PropertyGroup to move the PropertyType to</param>
    /// <returns></returns>
    bool MovePropertyType(string propertyTypeAlias, string propertyGroupAlias);

    /// <summary>
    ///     Gets an <see cref="ISimpleContentType" /> corresponding to this content type.
    /// </summary>
    ISimpleContentType ToSimple();
}
