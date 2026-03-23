namespace Umbraco.Cms.Core.Security;

/// <summary>
/// Provides an interface for managing client credentials associated with members.
/// </summary>
public interface IMemberClientCredentialsManager
{
    /// <summary>
    /// Asynchronously retrieves all member client credentials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with a result of an enumerable collection of <see cref="MemberClientCredentials"/>.</returns>
    Task<IEnumerable<MemberClientCredentials>> GetAllAsync();

    /// <summary>
    /// Asynchronously finds a member associated with the specified client ID.
    /// </summary>
    /// <param name="clientId">The client ID to search for.</param>
    /// <returns>A task representing the asynchronous operation. The result contains the <see cref="MemberIdentityUser"/> if found; otherwise, <c>null</c>.</returns>
    Task<MemberIdentityUser?> FindMemberAsync(string clientId);
}
