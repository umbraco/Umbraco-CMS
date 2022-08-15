using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Net;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Identity support for Umbraco back office
    /// </summary>
    public static IUmbracoBuilder AddBackOfficeIdentity(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.AddDataProtection();

        builder.BuildUmbracoBackOfficeIdentity()
            .AddDefaultTokenProviders()
            .AddUserStore<IUserStore<BackOfficeIdentityUser>, BackOfficeUserStore>(factory => new BackOfficeUserStore(
                factory.GetRequiredService<ICoreScopeProvider>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IEntityService>(),
                factory.GetRequiredService<IExternalLoginWithKeyService>(),
                factory.GetRequiredService<IOptionsSnapshot<GlobalSettings>>(),
                factory.GetRequiredService<IUmbracoMapper>(),
                factory.GetRequiredService<BackOfficeErrorDescriber>(),
                factory.GetRequiredService<AppCaches>(),
                factory.GetRequiredService<ITwoFactorLoginService>()
            ))
            .AddUserManager<IBackOfficeUserManager, BackOfficeUserManager>()
            .AddSignInManager<IBackOfficeSignInManager, BackOfficeSignInManager>()
            .AddClaimsPrincipalFactory<BackOfficeClaimsPrincipalFactory>()
            .AddErrorDescriber<BackOfficeErrorDescriber>();

        services.TryAddSingleton<IBackOfficeUserPasswordChecker, NoopBackOfficeUserPasswordChecker>();

        // Configure the options specifically for the UmbracoBackOfficeIdentityOptions instance
        services.ConfigureOptions<ConfigureBackOfficeIdentityOptions>();
        services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();

        return builder;
    }

    private static BackOfficeIdentityBuilder BuildUmbracoBackOfficeIdentity(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        services.TryAddScoped<IIpResolver, AspNetCoreIpResolver>();
        services.TryAddSingleton<IBackOfficeExternalLoginProviders, BackOfficeExternalLoginProviders>();
        services.TryAddSingleton<IBackOfficeTwoFactorOptions, DefaultBackOfficeTwoFactorOptions>();

        return new BackOfficeIdentityBuilder(services);
    }

    /// <summary>
    ///     Adds support for external login providers in Umbraco
    /// </summary>
    public static IUmbracoBuilder AddBackOfficeExternalLogins(this IUmbracoBuilder umbracoBuilder,
        Action<BackOfficeExternalLoginsBuilder> builder)
    {
        builder(new BackOfficeExternalLoginsBuilder(umbracoBuilder.Services));
        return umbracoBuilder;
    }

    public static BackOfficeIdentityBuilder AddTwoFactorProvider<T>(this BackOfficeIdentityBuilder identityBuilder,
        string providerName) where T : class, ITwoFactorProvider
    {
        identityBuilder.Services.AddSingleton<ITwoFactorProvider, T>();
        identityBuilder.Services.AddSingleton<T>();
        identityBuilder.AddTokenProvider<TwoFactorBackOfficeValidationProvider<T>>(providerName);

        return identityBuilder;
    }
}
