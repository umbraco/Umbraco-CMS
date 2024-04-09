namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class ContentTypeResponseModelBase<TPropertyType, TPropertyTypeContainer>
    : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public Guid Id { get; set; }
}
