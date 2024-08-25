namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class PasswordConfigurationResponseModel
{
    public int MinimumPasswordLength { get; set; }

    public bool RequireNonLetterOrDigit { get; set; }

    public bool RequireDigit { get; set; }

    public bool RequireLowercase { get; set; }

    public bool RequireUppercase { get; set; }
}
