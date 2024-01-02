namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class CurrenUserConfigurationResponseModel
{
    public bool KeepUserLoggedIn { get; set; }

    public bool UserNameIsEmail { get; set; }

    public int MinimumPasswordLength { get; set; }

    public int MinimumPasswordNonAlphaNum { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireUppercase { get; set; }

}
