namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class DocumentAllowedParentsResponseModel
{
    public required IEnumerable<Guid> AllowedParentsKeys { get; set; }
}
