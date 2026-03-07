using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Used to add back office login providers
/// </summary>
public class BackOfficeExternalLoginsBuilder
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeExternalLoginsBuilder"/> class for configuring external login providers in the backoffice.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which external login services will be added.</param>
    public BackOfficeExternalLoginsBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Add a back office login provider with options
    /// </summary>
    /// <param name="loginProviderOptions"></param>
    /// <param name="build"></param>
    /// <returns></returns>
    public BackOfficeExternalLoginsBuilder AddBackOfficeLogin(
        Action<BackOfficeAuthenticationBuilder> build,
        Action<BackOfficeExternalLoginProviderOptions>? loginProviderOptions = null)
    {
        build(new BackOfficeAuthenticationBuilder(_services, loginProviderOptions));
        return this;
    }
}
