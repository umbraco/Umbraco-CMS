using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IdentityBuilder" />
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    ///     Adds a <see cref="UserManager{TUser}" /> for the <seealso cref="MemberIdentityUser" />.
    /// </summary>
    /// <typeparam name="TInterface">The member manager interface</typeparam>
    /// <typeparam name="TUserManager">The member manager type</typeparam>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public static IdentityBuilder AddMemberManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder)
        where TUserManager : UserManager<MemberIdentityUser>, TInterface
        where TInterface : notnull
    {
        identityBuilder.AddUserManager<TUserManager>();

        // use a UniqueServiceDescriptor so we can check if it's already been added
        var memberManagerDescriptor =
            new UniqueServiceDescriptor(typeof(TInterface), typeof(TUserManager), ServiceLifetime.Scoped);
        identityBuilder.Services.Add(memberManagerDescriptor);
        identityBuilder.Services.AddScoped(
            typeof(UserManager<MemberIdentityUser>),
            factory => factory.GetRequiredService<TInterface>());
        return identityBuilder;
    }

    public static IdentityBuilder AddRoleManager<TInterface, TRoleManager>(this IdentityBuilder identityBuilder)
        where TRoleManager : RoleManager<UmbracoIdentityRole>, TInterface
        where TInterface : notnull
    {
        identityBuilder.AddRoleManager<TRoleManager>();
        identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TRoleManager));
        identityBuilder.Services.AddScoped(
            typeof(RoleManager<MemberIdentityUser>),
            factory => factory.GetRequiredService<TInterface>());
        return identityBuilder;
    }

    /// <summary>
    ///     Adds a <see cref="SignInManager{TUser}" /> implementation for <seealso cref="MemberIdentityUser" />
    /// </summary>
    /// <typeparam name="TInterface">The sign in manager interface</typeparam>
    /// <typeparam name="TSignInManager">The sign in manager type</typeparam>
    /// <param name="identityBuilder">The <see cref="IdentityBuilder" /></param>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public static IdentityBuilder AddSignInManager<TInterface, TSignInManager>(this IdentityBuilder identityBuilder)
        where TSignInManager : SignInManager<MemberIdentityUser>, TInterface
    {
        identityBuilder.AddSignInManager<TSignInManager>();
        identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TSignInManager));
        return identityBuilder;
    }

    public static IdentityBuilder AddUserStore<TInterface, TStore>(
        this IdentityBuilder identityBuilder,
        Func<IServiceProvider, TStore> implementationFactory)
        where TStore : class, TInterface
    {
        identityBuilder.Services.AddScoped(typeof(TInterface), implementationFactory);
        return identityBuilder;
    }

    public static MemberIdentityBuilder AddTwoFactorProvider<T>(
        this MemberIdentityBuilder identityBuilder,
        string providerName)
        where T : class, ITwoFactorProvider
    {
        identityBuilder.Services.AddSingleton<ITwoFactorProvider, T>();
        identityBuilder.Services.AddSingleton<T>();
        identityBuilder.AddTokenProvider<TwoFactorMemberValidationProvider<T>>(providerName);

        return identityBuilder;
    }
}
