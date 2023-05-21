using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Identity support for Umbraco members
    /// </summary>
    public static IUmbracoBuilder AddMembersIdentity(this IUmbracoBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // check if this has already been added, we cannot add twice but both front-end and back end
        // depend on this so it's possible it can be called twice.
        var distCacheBinder =
            new UniqueServiceDescriptor(typeof(IMemberManager), typeof(MemberManager), ServiceLifetime.Scoped);
        if (builder.Services.Contains(distCacheBinder))
        {
            return builder;
        }

        // NOTE: We are using AddIdentity which is going to add all of the default AuthN/AuthZ configurations = OK!
        // This will also add all of the default identity services for our user/role types that we aren't overriding = OK!
        // If a developer wishes to use Umbraco Members with different AuthN/AuthZ values, like different cookie values
        // or authentication scheme's then they can call the default identity configuration methods like ConfigureApplicationCookie.
        // BUT ... if a developer wishes to use the default auth schemes for entirely separate purposes alongside Umbraco members,
        // then we'll probably have to change this and make it more flexible like how we do for Users. Which means booting up
        // identity here with the basics and registering all of our own custom services.
        // Since we are using the defaults in v8 (and below) for members, I think using the default for members now is OK!
        services.AddIdentity<MemberIdentityUser, UmbracoIdentityRole>()
            .AddDefaultTokenProviders()
            .AddUserStore<IUserStore<MemberIdentityUser>, MemberUserStore>(factory => new MemberUserStore(
                factory.GetRequiredService<IMemberService>(),
                factory.GetRequiredService<IUmbracoMapper>(),
                factory.GetRequiredService<ICoreScopeProvider>(),
                factory.GetRequiredService<IdentityErrorDescriber>(),
                factory.GetRequiredService<IPublishedSnapshotAccessor>(),
                factory.GetRequiredService<IExternalLoginWithKeyService>(),
                factory.GetRequiredService<ITwoFactorLoginService>()))
            .AddRoleStore<MemberRoleStore>()
            .AddRoleManager<IMemberRoleManager, MemberRoleManager>()
            .AddMemberManager<IMemberManager, MemberManager>()
            .AddSignInManager<IMemberSignInManager, MemberSignInManager>()
            .AddClaimsPrincipalFactory<MemberClaimsPrincipalFactory>()
            .AddErrorDescriber<MembersErrorDescriber>()
            .AddUserConfirmation<UmbracoUserConfirmation<MemberIdentityUser>>();

        builder.AddNotificationHandler<MemberDeletedNotification, DeleteExternalLoginsOnMemberDeletedHandler>();
        builder.AddNotificationAsyncHandler<MemberDeletedNotification, DeleteTwoFactorLoginsOnMemberDeletedHandler>();
        services.ConfigureOptions<ConfigureMemberIdentityOptions>();

        services.AddScoped(x => (IMemberUserStore)x.GetRequiredService<IUserStore<MemberIdentityUser>>());
        services.AddScoped<IPasswordHasher<MemberIdentityUser>, MemberPasswordHasher>();

        services.ConfigureOptions<ConfigureSecurityStampOptions>();
        services.ConfigureOptions<ConfigureMemberCookieOptions>();

        services.AddUnique<IMemberExternalLoginProviders, MemberExternalLoginProviders>();

        return builder;
    }
}
