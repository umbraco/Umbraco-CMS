using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Security;

/// <summary>
///     Custom <see cref="AuthenticationBuilder" /> used to associate external logins with umbraco external login options
/// </summary>
public class MemberAuthenticationBuilder : AuthenticationBuilder
{
    private readonly Action<MemberExternalLoginProviderOptions> _loginProviderOptions;

    public MemberAuthenticationBuilder(
        IServiceCollection services,
        Action<MemberExternalLoginProviderOptions>? loginProviderOptions = null)
        : base(services)
        => _loginProviderOptions = loginProviderOptions ?? (x => { });

    public string SchemeForMembers(string scheme)
        => scheme.EnsureStartsWith(Constants.Security.MemberExternalAuthenticationTypePrefix);

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
        string authenticationScheme, string? displayName, Action<TOptions>? configureOptions)
    {
        // Validate that the prefix is set
        if (!authenticationScheme.StartsWith(Constants.Security.MemberExternalAuthenticationTypePrefix))
        {
            throw new InvalidOperationException(
                $"The {nameof(authenticationScheme)} is not prefixed with {Constants.Security.MemberExternalAuthenticationTypePrefix}. The scheme must be created with a call to the method {nameof(SchemeForMembers)}");
        }

        // add our login provider to the container along with a custom options configuration
        Services.Configure(authenticationScheme, _loginProviderOptions);
        Services.AddSingleton(services =>
        {
            return new MemberExternalLoginProvider(
                authenticationScheme,
                services.GetRequiredService<IOptionsMonitor<MemberExternalLoginProviderOptions>>());
        });
        Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IPostConfigureOptions<TOptions>, EnsureMemberScheme<TOptions>>());

        return base.AddRemoteScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
    }

    // Ensures that the sign in scheme is always the Umbraco member external type
    private class EnsureMemberScheme<TOptions> : IPostConfigureOptions<TOptions>
        where TOptions : RemoteAuthenticationOptions
    {
        public void PostConfigure(string name, TOptions options)
        {
            if (!name.StartsWith(Constants.Security.MemberExternalAuthenticationTypePrefix))
            {
                return;
            }

            options.SignInScheme = IdentityConstants.ExternalScheme;
        }
    }
}
