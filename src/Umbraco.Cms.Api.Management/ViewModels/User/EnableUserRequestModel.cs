namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class EnableUserRequestModel
{
    public SortedSet<Guid> UserIds { get; set; } = new();
}
