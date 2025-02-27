using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class CreateUserRequestModel : CreateUserRequestModelBase
{
    public UserKind Kind { get; set; }
}
