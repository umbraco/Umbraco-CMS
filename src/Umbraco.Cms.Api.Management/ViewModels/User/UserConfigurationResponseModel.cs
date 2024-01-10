using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserConfigurationResponseModel
{
    public bool ShowUserInvite { get; set; }

    public PasswordConfigurationResponseModel PasswordConfiguration { get; set; } = null!;
}
