using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Custom <see cref="AuthenticationBuilder" /> used to associate external logins with umbraco external login options
/// </summary>
public class BackOfficeAuthenticationBuilder : AuthenticationBuilder
{
    private readonly Action<BackOfficeExternalLoginProviderOptions> _loginProviderOptions;

    public BackOfficeAuthenticationBuilder(
        IServiceCollection services,
        Action<BackOfficeExternalLoginProviderOptions>? loginProviderOptions = null)
        : base(services)
        => _loginProviderOptions = loginProviderOptions ?? (x => { });

    public string? SchemeForBackOffice(string scheme)
        => scheme?.EnsureStartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix);

    /// <summary>
    ///     Overridden to track the final authenticationScheme being registered for the external login
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="authenticationScheme"></param>
    /// <param name="displayName"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public override AuthenticationBuilder AddRemoteScheme<TOptions, THandler>(string authenticationScheme,
        string? displayName, Action<TOptions>? configureOptions)
    {
        // Validate that the prefix is set
        if (!authenticationScheme.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix))
        {
            throw new InvalidOperationException(
                $"The {nameof(authenticationScheme)} is not prefixed with {Constants.Security.BackOfficeExternalAuthenticationTypePrefix}. The scheme must be created with a call to the method {nameof(SchemeForBackOffice)}");
        }

        // add our login provider to the container along with a custom options configuration
        Services.Configure(authenticationScheme, _loginProviderOptions);
        base.Services.AddSingleton(services =>
        {
            return new BackOfficeExternalLoginProvider(
                authenticationScheme,
                services.GetRequiredService<IOptionsMonitor<BackOfficeExternalLoginProviderOptions>>());
        });
        Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IPostConfigureOptions<TOptions>, EnsureBackOfficeScheme<TOptions>>());

        return base.AddRemoteScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
    }

    // TODO: We could override and throw NotImplementedException for other methods?

    // Ensures that the sign in scheme is always the Umbraco back office external type
    internal class EnsureBackOfficeScheme<TOptions> : IPostConfigureOptions<TOptions>
        where TOptions : RemoteAuthenticationOptions
    {
        public void PostConfigure(string name, TOptions options)
        {
            // ensure logic only applies to backoffice authentication schemes
            if (name.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix))
            {
                options.SignInScheme = Constants.Security.BackOfficeExternalAuthenticationType;
            }
        }
    }
}
