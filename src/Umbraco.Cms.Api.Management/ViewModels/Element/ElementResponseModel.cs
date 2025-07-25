namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public class ElementResponseModel : ElementResponseModelBase<ElementValueResponseModel, ElementVariantResponseModel>
{
    public bool IsTrashed { get; set; }
}
