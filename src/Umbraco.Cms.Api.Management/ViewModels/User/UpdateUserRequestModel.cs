namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserRequestModel : UserPresentationBase
{
    public string LanguageIsoCode { get; set; } = string.Empty;

    public ISet<ReferenceByIdModel> DocumentStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    public bool HasDocumentRootAccess { get; init; }

    public ISet<ReferenceByIdModel> MediaStartNodeIds { get; set; } = new HashSet<ReferenceByIdModel>();

    public bool HasMediaRootAccess { get; init; }
}
