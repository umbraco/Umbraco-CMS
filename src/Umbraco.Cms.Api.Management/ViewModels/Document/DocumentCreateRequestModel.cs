using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentCreateRequestModel : ContentCreateRequestModelBase<DocumentValueModel, DocumentVariantModelBase>
{
    public Guid ContentTypeKey { get; set; }

    public Guid? TemplateKey { get; set; }
}
