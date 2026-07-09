namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base class for request models used to create content types, parameterized by property type and property container type.
/// </summary>
/// <typeparam name="TPropertyType">The type representing a property within the content type.</typeparam>
/// <typeparam name="TPropertyTypeContainer">The type representing a container for properties within the content type.</typeparam>
public abstract class CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the content type.
    /// </summary>
    public Guid? Id { get; set; }
}
