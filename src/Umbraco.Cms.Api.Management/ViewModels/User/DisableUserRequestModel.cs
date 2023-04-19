namespace Umbraco.Cms.Api.Management.ViewModels.Users;

public class DisableUserRequestModel
{
    public SortedSet<Guid> UserIds { get; set; } = new();
 }
