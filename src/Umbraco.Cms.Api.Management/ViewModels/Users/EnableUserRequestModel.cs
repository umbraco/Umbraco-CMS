namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class EnableUserRequestModel
{
    public SortedSet<Guid> UserKeys { get; set; } = new();
}
