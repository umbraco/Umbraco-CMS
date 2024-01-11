using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrenUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    public bool UsernameIsEmail { get; set; }

    public required PasswordConfigurationResponseModel PasswordConfiguration { get; set; }
}
