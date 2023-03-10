namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UnlockUsersRequestModel
{
    public SortedSet<Guid> UserKeys { get; set; } = new();
}
