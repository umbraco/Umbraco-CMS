namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class MediaTypeAllowedParentsResponseModel
{
    public required IEnumerable<Guid> AllowedParentsKeys { get; set; }
}
