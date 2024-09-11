namespace Umbraco.Cms.Api.Management.ViewModels.Security;

public class RequiresTwoFactorResponseModel
{
    public string? TwoFactorLoginView { get; set; }
    public required IEnumerable<string> EnabledTwoFactorProviderNames { get; set; }
}
