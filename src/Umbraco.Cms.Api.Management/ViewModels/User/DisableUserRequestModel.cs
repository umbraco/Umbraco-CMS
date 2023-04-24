namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class DisableUserRequestModel
{
    public SortedSet<Guid> UserIds { get; set; } = new();
 }
