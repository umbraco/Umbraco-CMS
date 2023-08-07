namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : ContentTypeModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public Guid? Key { get; set; }

    public Guid? ParentKey { get; set; }
}
