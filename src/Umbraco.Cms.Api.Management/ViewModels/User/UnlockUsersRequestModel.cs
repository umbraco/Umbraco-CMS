namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UnlockUsersRequestModel
{
    public SortedSet<Guid> UserIds { get; set; } = new();
}
