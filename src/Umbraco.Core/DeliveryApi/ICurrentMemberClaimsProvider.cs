namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a provider that retrieves claims for the currently logged in member.
/// </summary>
public interface ICurrentMemberClaimsProvider
{
    /// <summary>
    /// Retrieves the claims for the currently logged in member.
    /// </summary>
    /// <remarks>
    /// This is used by the OIDC user info endpoint to supply "current user" info.
    /// </remarks>
    Task<Dictionary<string, object>> GetClaimsAsync();
}
