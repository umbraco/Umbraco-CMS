using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

// TODO (V16): Correct the spelling on this class name, it should be CurrentUserConfigurationResponseModel.
public class CurrenUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    [Obsolete("Use the UserConfigurationResponseModel instead. This will be removed in V15.")]
    public bool UsernameIsEmail { get; set; }

    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }

    public bool AllowChangePassword { get; set; }

    public bool AllowTwoFactor { get; set; }
}
