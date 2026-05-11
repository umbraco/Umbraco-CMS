namespace Umbraco.Cms.Core.Models.ContentTypeEditing;

/// <summary>
/// <para>
/// This is the common model for all content types, Documents, Media, Members.
/// All the properties are common across all content types. Additionally most properties of the property types are shared as well.
/// </para>
/// To allow for common validation and such, the property types are required to inherit from the same base.
/// The same goes for the property type containers. (I.E Tabs and Groups)
/// </summary>
/// <typeparam name="TPropertyType">The type of the property types, I.E <see cref="ContentTypePropertyTypeModel"/>.</typeparam>
/// <typeparam name="TPropertyTypeContainer">The type of the content type containers, I.E <see cref="ContentTypePropertyContainerModel"/>.</typeparam>
public abstract class ContentTypeEditingModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    /// <summary>
    ///     Gets or sets the alias of the content type.
    /// </summary>
    public string Alias { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the display name of the content type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the description of the content type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the icon identifier for the content type.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether content of this type can be created at the root level.
    /// </summary>
    public bool AllowedAsRoot { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether content of this type varies by culture.
    /// </summary>
    public bool VariesByCulture { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether content of this type varies by segment.
    /// </summary>
    public bool VariesBySegment { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this content type is an element type.
    /// </summary>
    public bool IsElement { get; set; }

    /// <summary>
    ///     Gets or sets the key of the list view data type associated with this content type.
    /// </summary>
    public Guid? ListView { get; set; }

    /// <summary>
    ///     Gets or sets the property types defined on this content type.
    /// </summary>
    public IEnumerable<TPropertyType> Properties { get; set; } = Array.Empty<TPropertyType>();

    /// <summary>
    ///     Gets or sets the property type containers (tabs and groups) defined on this content type.
    /// </summary>
    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Array.Empty<TPropertyTypeContainer>();

    /// <summary>
    ///     Gets or sets the content types that are allowed to be created as children of content of this type.
    /// </summary>
    public IEnumerable<ContentTypeSort> AllowedContentTypes { get; set; } = Array.Empty<ContentTypeSort>();

    /// <summary>
    ///     Gets or sets the compositions that this content type uses.
    /// </summary>
    public IEnumerable<Composition> Compositions { get; set; } = Array.Empty<Composition>();
}
