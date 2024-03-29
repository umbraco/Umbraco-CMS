using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserConfigurationResponseModel
{
    public bool CanInviteUsers { get; set; }

    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
