namespace Umbraco.Cms.Core.Security;

public interface IMemberClientCredentialsManager
{
    Task<IEnumerable<MemberClientCredentials>> GetAllAsync();

    Task<MemberIdentityUser?> FindMemberAsync(string clientId);
}
