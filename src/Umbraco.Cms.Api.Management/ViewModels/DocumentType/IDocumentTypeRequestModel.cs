using Umbraco.Cms.Api.Management.ViewModels.ContentType;

namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public interface IDocumentTypeRequestModel
{
    IEnumerable<Guid> AllowedTemplateIds { get; set; }

    Guid? DefaultTemplateId { get; set; }

    ContentTypeCleanup Cleanup { get; set; }
}
