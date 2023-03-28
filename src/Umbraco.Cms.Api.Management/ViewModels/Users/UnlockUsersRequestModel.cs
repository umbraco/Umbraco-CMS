namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class UnlockUsersRequestModel
{
    public SortedSet<Guid> UserIds { get; set; } = new();
}
