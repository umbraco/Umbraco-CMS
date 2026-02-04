namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ElementVersionResponseModel : ElementResponseModelBase<ElementValueResponseModel, ElementVariantResponseModel>
{
    public ReferenceByIdModel? Element { get; set; }
}
