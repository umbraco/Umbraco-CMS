namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UpdateUserRequestModel : UserPresentationBase
{
    public string Language { get; set; } = string.Empty;

    public SortedSet<Guid> ContentStartNodeKeys { get; set; } = new();

    public SortedSet<Guid> MediaStartNodeKeys { get; set; } = new();
}
