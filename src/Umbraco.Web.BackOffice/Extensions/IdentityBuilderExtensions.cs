using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IdentityBuilder" />
/// </summary>
public static class IdentityBuilderExtensions
{
    /// <summary>
    ///     Adds a <see cref="UserManager{TUser}" /> implementation for <seealso cref="BackOfficeIdentityUser" />
    /// </summary>
    /// <typeparam name="TInterface">The usermanager interface</typeparam>
    /// <typeparam name="TUserManager">The usermanager type</typeparam>
    /// <param name="identityBuilder">The <see cref="IdentityBuilder" /></param>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public static IdentityBuilder AddUserManager<TInterface, TUserManager>(this IdentityBuilder identityBuilder)
        where TUserManager : UserManager<BackOfficeIdentityUser>, TInterface
    {
        identityBuilder.AddUserManager<TUserManager>();
        identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TUserManager));
        return identityBuilder;
    }

    /// <summary>
    ///     Adds a <see cref="SignInManager{TUser}" /> implementation for <seealso cref="BackOfficeIdentityUser" />
    /// </summary>
    /// <typeparam name="TInterface">The sign in manager interface</typeparam>
    /// <typeparam name="TSignInManager">The sign in manager type</typeparam>
    /// <param name="identityBuilder">The <see cref="IdentityBuilder" /></param>
    /// <returns>The current <see cref="IdentityBuilder" /> instance.</returns>
    public static IdentityBuilder AddSignInManager<TInterface, TSignInManager>(this IdentityBuilder identityBuilder)
        where TSignInManager : SignInManager<BackOfficeIdentityUser>, TInterface
    {
        identityBuilder.AddSignInManager<TSignInManager>();
        identityBuilder.Services.AddScoped(typeof(TInterface), typeof(TSignInManager));
        return identityBuilder;
    }
}
