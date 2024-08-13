namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class EnableUserRequestModel
{
    public ISet<ReferenceByIdModel> UserIds { get; set; } = new HashSet<ReferenceByIdModel>();
}
