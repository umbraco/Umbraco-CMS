using Umbraco.Cms.Api.Management.ViewModels.Content;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentUpdateModel : ContentUpdateModelBase<DocumentValueModel, DocumentVariantModel>
{
    public Guid? TemplateKey { get; set; }
}
