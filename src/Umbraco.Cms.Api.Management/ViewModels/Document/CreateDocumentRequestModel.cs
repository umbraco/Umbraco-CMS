using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class CreateDocumentRequestModel : CreateContentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    public Guid ContentTypeId { get; set; }

    public Guid? TemplateId { get; set; }
}
