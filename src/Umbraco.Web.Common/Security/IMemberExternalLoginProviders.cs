namespace Umbraco.Cms.Web.Common.Security;

/// <summary>
///     Service to return <see cref="MemberExternalLoginProvider" /> instances
/// </summary>
public interface IMemberExternalLoginProviders
{
    /// <summary>
    ///     Get the <see cref="BackOfficeExternalLoginProvider" /> for the specified scheme
    /// </summary>
    /// <param name="authenticationType"></param>
    /// <returns></returns>
    Task<MemberExternalLoginProviderScheme?> GetAsync(string authenticationType);

    /// <summary>
    ///     Get all registered <see cref="BackOfficeExternalLoginProvider" />
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<MemberExternalLoginProviderScheme>> GetMemberProvidersAsync();
}
