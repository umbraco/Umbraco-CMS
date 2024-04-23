namespace Umbraco.Cms.Api.Management.ViewModels.User.Current;

public class UserExternalLoginProviderResponseModel
{
    public required string ProviderSchemaName { get; set; }

    public bool IsLinkedOnUser { get; set; }

    public bool HasManualLinkingEnabled { get; set; }
}
