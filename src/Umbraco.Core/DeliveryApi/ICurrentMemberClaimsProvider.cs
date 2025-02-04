namespace Umbraco.Cms.Core.DeliveryApi;

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
