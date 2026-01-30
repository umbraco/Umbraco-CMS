namespace Umbraco.Cms.Api.Management.ViewModels.DocumentType;

public class DocumentTypeAllowedParentsResponseModel
{
    public required IEnumerable<Guid> AllowedParentsKeys { get; set; }
}
