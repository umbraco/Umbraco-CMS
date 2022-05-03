using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

public static partial class UmbracoApplicationBuilderExtensions
{
    public static IUmbracoBuilder SetBackOfficeUserManager<TUserManager>(this IUmbracoBuilder builder)
        where TUserManager : UserManager<BackOfficeIdentityUser>, IBackOfficeUserManager
    {
        Type customType = typeof(TUserManager);
        Type userManagerType = typeof(UserManager<BackOfficeIdentityUser>);
        builder.Services.Replace(ServiceDescriptor.Scoped(typeof(IBackOfficeUserManager), customType));
        builder.Services.AddScoped(customType, services => services.GetRequiredService(userManagerType));
        builder.Services.Replace(ServiceDescriptor.Scoped(userManagerType, customType));
        return builder;
    }

    public static IUmbracoBuilder SetBackOfficeUserStore<TUserStore>(this IUmbracoBuilder builder)
        where TUserStore : BackOfficeUserStore
    {
        Type customType = typeof(TUserStore);
        builder.Services.Replace(
            ServiceDescriptor.Scoped(typeof(IUserStore<>).MakeGenericType(typeof(BackOfficeIdentityUser)), customType));
        return builder;
    }

    public static IUmbracoBuilder SetMemberManager<TUserManager>(this IUmbracoBuilder builder)
        where TUserManager : UserManager<MemberIdentityUser>, IMemberManager
    {
        Type customType = typeof(TUserManager);
        Type userManagerType = typeof(UserManager<MemberIdentityUser>);
        builder.Services.Replace(ServiceDescriptor.Scoped(typeof(IMemberManager), customType));
        builder.Services.AddScoped(customType, services => services.GetRequiredService(userManagerType));
        builder.Services.Replace(ServiceDescriptor.Scoped(userManagerType, customType));
        return builder;
    }

    public static IUmbracoBuilder SetMemberUserStore<TUserStore>(this IUmbracoBuilder builder)
        where TUserStore : MemberUserStore
    {
        Type customType = typeof(TUserStore);
        builder.Services.Replace(
            ServiceDescriptor.Scoped(typeof(IUserStore<>).MakeGenericType(typeof(MemberIdentityUser)), customType));
        return builder;
    }
}
