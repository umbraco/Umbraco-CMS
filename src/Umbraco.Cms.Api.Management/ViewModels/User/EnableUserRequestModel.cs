namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class EnableUserRequestModel
{
    public ISet<Guid> UserIds { get; set; } = new HashSet<Guid>();
}
