using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
///     Custom <see cref="AuthenticationBuilder" /> used to associate external logins with umbraco external login options
/// </summary>
public class BackOfficeAuthenticationBuilder : AuthenticationBuilder
{
    private readonly Action<BackOfficeExternalLoginProviderOptions> _loginProviderOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackOfficeAuthenticationBuilder"/> class.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which authentication services will be added.</param>
    /// <param name="loginProviderOptions">An optional action to configure external login provider options.</param>
    public BackOfficeAuthenticationBuilder(
        IServiceCollection services,
        Action<BackOfficeExternalLoginProviderOptions>? loginProviderOptions = null)
        : base(services)
        => _loginProviderOptions = loginProviderOptions ?? (x => { });

    // TODO (V18): Change the return value to be not nullable and remove the NotNullIfNotNull attribute.
    /// <summary>
    /// Returns the specified authentication scheme prefixed with the back office external authentication type prefix.
    /// </summary>
    /// <param name="scheme">The authentication scheme to prefix.</param>
    /// <returns>The prefixed scheme, or <c>null</c> if <paramref name="scheme"/> is <c>null</c>.</returns>
    [return: NotNullIfNotNull(nameof(scheme))]
    public static string? SchemeForBackOffice(string scheme)
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
    public override AuthenticationBuilder AddRemoteScheme<TOptions, THandler>(
        string authenticationScheme,
        string? displayName,
        Action<TOptions>? configureOptions)
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
    internal sealed class EnsureBackOfficeScheme<TOptions> : IPostConfigureOptions<TOptions>
        where TOptions : RemoteAuthenticationOptions
    {
    /// <summary>
    /// Post-configures the specified options instance for a given authentication scheme name.
    /// If the scheme name indicates a backoffice authentication scheme, sets the <c>SignInScheme</c> property
    /// to the backoffice external authentication type. This ensures the configuration is only applied to backoffice schemes.
    /// </summary>
    /// <param name="name">The name of the authentication scheme to configure.</param>
    /// <param name="options">The options instance to configure. If the scheme is a backoffice scheme, its <c>SignInScheme</c> will be set.</param>
        public void PostConfigure(string? name, TOptions options)
        {
            // ensure logic only applies to backoffice authentication schemes
            if (name is not null && name.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix))
            {
                options.SignInScheme = Constants.Security.BackOfficeExternalAuthenticationType;
            }
        }
    }
}
