using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentUpdateRequestModel : ContentUpdateRequestModelBase<DocumentValueModel, DocumentVariantModelBase>
{
    public Guid? TemplateKey { get; set; }
}
