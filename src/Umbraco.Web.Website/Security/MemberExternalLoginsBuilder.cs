using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Cms.Web.Website.Security;

/// <summary>
///     Used to add back office login providers
/// </summary>
public class MemberExternalLoginsBuilder
{
    private readonly IServiceCollection _services;

    public MemberExternalLoginsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Add a back office login provider with options
    /// </summary>
    /// <param name="loginProviderOptions"></param>
    /// <param name="build"></param>
    /// <returns></returns>
    public MemberExternalLoginsBuilder AddMemberLogin(
        Action<MemberAuthenticationBuilder> build,
        Action<MemberExternalLoginProviderOptions>? loginProviderOptions = null)
    {
        build(new MemberAuthenticationBuilder(_services, loginProviderOptions));
        return this;
    }
}
