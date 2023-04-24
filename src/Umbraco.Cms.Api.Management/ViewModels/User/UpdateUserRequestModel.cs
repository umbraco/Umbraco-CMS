namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UpdateUserRequestModel : UserPresentationBase
{
    public string LanguageIsoCode { get; set; } = string.Empty;

    public SortedSet<Guid> ContentStartNodeIds { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeIds { get; set; } = new();
}
