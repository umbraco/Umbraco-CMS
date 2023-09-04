namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class PublishDocumentWithDescendantsRequestModel : PublishDocumentRequestModel
{
    public bool IncludeUnpublishedDescendants { get; set; }
}
