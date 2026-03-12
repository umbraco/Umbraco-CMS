using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Used to add back office login providers
/// </summary>
public class BackOfficeExternalLoginsBuilder
{
    private readonly IServiceCollection _services;

    public BackOfficeExternalLoginsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Add a back office login provider with options
    /// </summary>
    /// <param name="loginProviderOptions">Optional configuration for the login provider.</param>
    /// <param name="build">The builder action to configure the authentication scheme.</param>
    /// <returns>The builder for chaining.</returns>
    public BackOfficeExternalLoginsBuilder AddBackOfficeLogin(
        Action<BackOfficeAuthenticationBuilder> build,
        Action<BackOfficeExternalLoginProviderOptions>? loginProviderOptions = null)
    {
        build(new BackOfficeAuthenticationBuilder(_services, loginProviderOptions));
        return this;
    }
}
