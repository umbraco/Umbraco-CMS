namespace Umbraco.Cms.Api.Management.ViewModels.User;

public class UserConfigurationResponseModel
{
    public bool ShowUserInvite { get; set; }

    public int MinimumPasswordLength { get; set; }

    public bool MinimumPasswordNonAlphaNum { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireUppercase { get; set; }
}
