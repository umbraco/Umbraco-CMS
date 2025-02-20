namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class PublishDocumentWithDescendantsRequestModel
{
    public bool IncludeUnpublishedDescendants { get; set; }

    public bool ForceRepublish { get; set; }

    public required IEnumerable<string> Cultures { get; set; }
}
