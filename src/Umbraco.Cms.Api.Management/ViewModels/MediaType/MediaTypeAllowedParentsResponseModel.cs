namespace Umbraco.Cms.Api.Management.ViewModels.MediaType;

public class MediaTypeAllowedParentsResponseModel
{
    public required ISet<ReferenceByIdModel> AllowedParentIds { get; set; }
}
