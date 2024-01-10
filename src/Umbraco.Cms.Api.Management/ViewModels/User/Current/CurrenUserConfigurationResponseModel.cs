using Umbraco.Cms.Api.Management.ViewModels.Security;

namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrenUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    public bool UserNameIsEmail { get; set; }

    public PasswordConfigurationResponseModel PasswordConfiguration { get; set; } = null!;

}
