namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserRequestModel : UserPresentationBase
{
    public string LanguageIsoCode { get; set; } = string.Empty;

    public ISet<Guid> DocumentStartNodeIds { get; set; } = new HashSet<Guid>();

    public bool HasDocumentRootAccess { get; init; }

    public ISet<Guid> MediaStartNodeIds { get; set; } = new HashSet<Guid>();

    public bool HasMediaRootAccess { get; init; }
}
