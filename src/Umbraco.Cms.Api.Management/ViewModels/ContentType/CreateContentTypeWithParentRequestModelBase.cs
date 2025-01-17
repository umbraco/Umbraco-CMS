namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class CreateContentTypeWithParentRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public ReferenceByIdModel? Parent { get; set; }
}
