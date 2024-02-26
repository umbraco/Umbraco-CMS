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
    public string Alias { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Icon { get; set; } = string.Empty;

    public bool AllowedAsRoot { get; set; }

    public bool VariesByCulture { get; set; }

    public bool VariesBySegment { get; set; }

    public bool IsElement { get; set; }

    public Guid? ListView { get; set; }

    public IEnumerable<TPropertyType> Properties { get; set; } = Array.Empty<TPropertyType>();

    public IEnumerable<TPropertyTypeContainer> Containers { get; set; } = Array.Empty<TPropertyTypeContainer>();

    public IEnumerable<ContentTypeSort> AllowedContentTypes { get; set; } = Array.Empty<ContentTypeSort>();

    public IEnumerable<Composition> Compositions { get; set; } = Array.Empty<Composition>();
}
