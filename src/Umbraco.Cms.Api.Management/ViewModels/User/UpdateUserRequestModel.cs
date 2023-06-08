namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserRequestModel : UserPresentationBase
{
    public string LanguageIsoCode { get; set; } = string.Empty;

    public ISet<Guid> ContentStartNodeIds { get; set; } = new HashSet<Guid>();

    public ISet<Guid> MediaStartNodeIds { get; set; } = new HashSet<Guid>();
}
