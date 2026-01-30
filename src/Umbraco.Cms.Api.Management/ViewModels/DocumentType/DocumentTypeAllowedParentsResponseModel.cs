namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentTypeAllowedParentsResponseModel
{
    public required IEnumerable<Guid> AllowedParentsKeys { get; set; }
}
