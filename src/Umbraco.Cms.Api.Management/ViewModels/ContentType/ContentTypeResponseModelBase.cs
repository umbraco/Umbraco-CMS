namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as the base response model for content types, allowing customization of property types and their containers through generics.
/// </summary>
/// <typeparam name="TPropertyType">The type representing a property within the content type.</typeparam>
/// <typeparam name="TPropertyTypeContainer">The type representing a container for properties within the content type.</typeparam>
public abstract class ContentTypeResponseModelBase<TPropertyType, TPropertyTypeContainer>
    : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier for the content type.
    /// </summary>
    public Guid Id { get; set; }
}
