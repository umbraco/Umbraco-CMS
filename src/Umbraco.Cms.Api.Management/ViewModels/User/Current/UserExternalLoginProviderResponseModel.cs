namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserExternalLoginProviderResponseModel
{
    public required string ProviderSchemeName { get; set; }

    public bool IsLinkedOnUser { get; set; }

    public bool HasManualLinkingEnabled { get; set; }

    public string? ProviderKey { get; set; }
}
