namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class PublishDocumentRequestModel
{
    public required IEnumerable<string> Cultures { get; set; }
}
