using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions
{
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Adds Identity support for Umbraco members
        /// </summary>
        public static IUmbracoBuilder AddMembersIdentity(this IUmbracoBuilder builder)
        {
            IServiceCollection services = builder.Services;

            // check if this has already been added, we cannot add twice but both front-end and back end
            // depend on this so it's possible it can be called twice.
            var distCacheBinder = new UniqueServiceDescriptor(typeof(IMemberManager), typeof(MemberManager), ServiceLifetime.Scoped);
            if (builder.Services.Contains(distCacheBinder))
            {
                return builder;
            }

            // TODO: We may need to use services.AddIdentityCore instead if this is doing too much

            services.AddIdentity<MemberIdentityUser, UmbracoIdentityRole>()
                .AddDefaultTokenProviders()
                .AddUserStore<MemberUserStore>()
                .AddRoleStore<MemberRoleStore>()
                .AddRoleManager<IMemberRoleManager, MemberRoleManager>()
                .AddMemberManager<IMemberManager, MemberManager>()
                .AddSignInManager<IMemberSignInManager, MemberSignInManager>()
                .AddErrorDescriber<MembersErrorDescriber>();

            services.ConfigureApplicationCookie(x =>
            {
                // TODO: We may want/need to configure these further

                x.LoginPath = null;
                x.AccessDeniedPath = null;
                x.LogoutPath = null;
            });

            return builder;
        }
    }
}
