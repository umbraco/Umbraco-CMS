using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class VerifyInviteUserResponseModel
{
    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
