using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class UpdateDocumentRequestModel : ContentUpdateRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public Guid? TemplateKey { get; set; }
}
