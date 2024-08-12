using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CreateUserRequestModel : UserPresentationBase
{
    public Guid? Id { get; set; }

    public UserType Type { get; set; }
}
