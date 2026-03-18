namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

/// <summary>
/// Serves as a base request model for creating a content type with a parent, using generic property types and property type containers.
/// </summary>
/// <typeparam name="TPropertyType">The type representing a property.</typeparam>
/// <typeparam name="TPropertyTypeContainer">The type representing a container for property types.</typeparam>
public abstract class CreateContentTypeWithParentRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    /// <summary>
    /// Gets or sets the model representing the parent content type by its ID.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
