namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public class MediaTypeAllowedParentsResponseModel
{
    public required IEnumerable<Guid> AllowedParentsKeys { get; set; }
}
