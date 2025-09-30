namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class ValidateUpdateDocumentRequestModel : UpdateDocumentRequestModel
{
    public ISet<string>? Cultures { get; set; }
}
