using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrentUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }

    public bool AllowChangePassword { get; set; }

    public bool AllowTwoFactor { get; set; }
}
