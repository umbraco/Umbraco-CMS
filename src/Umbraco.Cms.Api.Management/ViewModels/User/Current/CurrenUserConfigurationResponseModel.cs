using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrenUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    [Obsolete("Use the UserConfigurationResponseModel instead. This will be removed in V15.")]
    public bool UsernameIsEmail { get; set; }

    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
