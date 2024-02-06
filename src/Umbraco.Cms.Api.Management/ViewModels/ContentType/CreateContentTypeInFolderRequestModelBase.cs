namespace Umbraco.Cms.Api.Management.ViewModels.ContentType;

public abstract class CreateContentTypeInFolderRequestModelBase<TPropertyType, TPropertyTypeContainer>
    : CreateContentTypeRequestModelBase<TPropertyType, TPropertyTypeContainer>
    where TPropertyType : PropertyTypeModelBase
    where TPropertyTypeContainer : PropertyTypeContainerModelBase
{
    public ReferenceByIdModel? Folder { get; set; }
}
