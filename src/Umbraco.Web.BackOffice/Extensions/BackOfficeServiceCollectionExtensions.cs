using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Security;
using Umbraco.Core.Serialization;
using Umbraco.Infrastructure.BackOffice;
using Umbraco.Net;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.Security;

namespace Umbraco.Extensions
{
    public static class BackOfficeServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the services required for running the Umbraco back office
        /// </summary>
        /// <param name="services"></param>
        public static void AddUmbracoBackOffice(this IServiceCollection services)
        {
            services.AddAntiforgery();

            // TODO: We had this check in v8 where we don't enable these unless we can run...
            //if (runtimeState.Level != RuntimeLevel.Upgrade && runtimeState.Level != RuntimeLevel.Run) return app;

            services.AddSingleton<IFilterProvider, OverrideAuthorizationFilterProvider>();
            services
                .AddAuthentication(Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Constants.Security.BackOfficeAuthenticationType)
                .AddCookie(Constants.Security.BackOfficeExternalAuthenticationType, o =>
                 {
                     o.Cookie.Name = Constants.Security.BackOfficeExternalAuthenticationType;
                     o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                 });

            // TODO: Need to add more cookie options, see https://github.com/dotnet/aspnetcore/blob/3.0/src/Identity/Core/src/IdentityServiceCollectionExtensions.cs#L45

            services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();

            services.AddBackOfficeAuthorizationPolicies();
        }

        public static void AddUmbracoPreview(this IServiceCollection services)
        {
            services.AddSignalR();
        }

        /// <summary>
        /// Adds the services required for using Umbraco back office Identity
        /// </summary>
        /// <param name="services"></param>
        public static void AddUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            services.AddDataProtection();

            services.BuildUmbracoBackOfficeIdentity()
                .AddDefaultTokenProviders()
                .AddUserStore<BackOfficeUserStore>()
                .AddUserManager<IBackOfficeUserManager, BackOfficeUserManager>()
                .AddSignInManager<BackOfficeSignInManager>()
                .AddClaimsPrincipalFactory<BackOfficeClaimsPrincipalFactory<BackOfficeIdentityUser>>();

            // Configure the options specifically for the UmbracoBackOfficeIdentityOptions instance
            services.ConfigureOptions<ConfigureBackOfficeIdentityOptions>();
            services.ConfigureOptions<ConfigureBackOfficeSecurityStampValidatorOptions>();
        }

        private static BackOfficeIdentityBuilder BuildUmbracoBackOfficeIdentity(this IServiceCollection services)
        {
            // Borrowed from https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/IdentityServiceCollectionExtensions.cs#L33
            // The reason we need our own is because the Identity system doesn't cater easily for multiple identity systems and particularly being
            // able to configure IdentityOptions to a specific provider since there is no named options. So we have strongly typed options
            // and strongly typed ILookupNormalizer and IdentityErrorDescriber since those are 'global' and we need to be unintrusive.

            // TODO: Could move all of this to BackOfficeComposer?

            // Services used by identity
            services.TryAddScoped<IUserValidator<BackOfficeIdentityUser>, UserValidator<BackOfficeIdentityUser>>();
            services.TryAddScoped<IPasswordValidator<BackOfficeIdentityUser>, PasswordValidator<BackOfficeIdentityUser>>();
            services.TryAddScoped<IPasswordHasher<BackOfficeIdentityUser>>(
                services => new BackOfficePasswordHasher(
                    new LegacyPasswordSecurity(),
                    services.GetRequiredService<IJsonSerializer>()));
            services.TryAddScoped<IUserConfirmation<BackOfficeIdentityUser>, DefaultUserConfirmation<BackOfficeIdentityUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<BackOfficeIdentityUser>, UserClaimsPrincipalFactory<BackOfficeIdentityUser>>();

            // CUSTOM:
            services.TryAddScoped<BackOfficeLookupNormalizer>();
            services.TryAddScoped<BackOfficeIdentityErrorDescriber>();
            services.TryAddScoped<IIpResolver, AspNetCoreIpResolver>();
            services.TryAddSingleton<IBackOfficeExternalLoginProviders, NopBackOfficeExternalLoginProviders>();

            /*
             * IdentityBuilderExtensions.AddUserManager adds UserManager<BackOfficeIdentityUser> to service collection
             * To validate the container the following registrations are required (dependencies of UserManager<T>)
             * Perhaps we shouldn't be registering UserManager<T> at all and only registering/depending the UmbracoBackOffice prefixed types.
             */
            services.TryAddScoped<ILookupNormalizer, BackOfficeLookupNormalizer>();
            services.TryAddScoped<IdentityErrorDescriber, BackOfficeIdentityErrorDescriber>();

            return new BackOfficeIdentityBuilder(services);
        }

        /// <summary>
        /// Add authorization handlers and policies
        /// </summary>
        /// <param name="services"></param>
        private static void AddBackOfficeAuthorizationPolicies(this IServiceCollection services)
        {
            // NOTE: Even though we are registering these handlers globally they will only actually execute their logic for
            // any auth defining a matching requirement. We don't want to get in the way of end-users own aspnet logic
            // and although these will trigger for any of their requests that need authorization, the logic won't actually execute.
            // Basically all registered IAuthorizationHandler will execute for all requests requiring authorization but their logic
            // won't trigger unless the requirement/policy matches.

            services.AddSingleton<IAuthorizationHandler, UmbracoTreeAuthorizeHandler>();
            services.AddSingleton<IAuthorizationHandler, UmbracoSectionAuthorizeHandler>();
            services.AddSingleton<IAuthorizationHandler, AdminUsersAuthorizeHandler>();
            services.AddSingleton<IAuthorizationHandler, DenyLocalLoginAuthorizeHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.AdminUserEditsRequireAdmin, policy =>
                {
                    policy.Requirements.Add(new AdminUsersAuthorizeRequirement());
                    policy.Requirements.Add(new AdminUsersAuthorizeRequirement("userIds"));
                });

                options.AddPolicy(AuthorizationPolicies.DenyLocalLoginIfConfigured, policy =>
                    policy.Requirements.Add(new DenyLocalLoginRequirement()));

                options.AddPolicy(AuthorizationPolicies.SectionAccessContent, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Content)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessContentOrMedia, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Content, Constants.Applications.Media)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessUsers, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Users)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessForTinyMce, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(
                        Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessMedia, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Media)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessMembers, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Members)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessPackages, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Packages)));

                options.AddPolicy(AuthorizationPolicies.SectionAccessSettings, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(Constants.Applications.Settings)));

                //We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
                // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
                options.AddPolicy(AuthorizationPolicies.SectionAccessForContentTree, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(
                        Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
                        Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members)));
                options.AddPolicy(AuthorizationPolicies.SectionAccessForMediaTree, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(
                        Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
                        Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members)));
                options.AddPolicy(AuthorizationPolicies.SectionAccessForMemberTree, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(
                        Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members)));

                // Permission is granted to this policy if the user has access to any of these sections: Content, media, settings, developer, members
                options.AddPolicy(AuthorizationPolicies.SectionAccessForDataTypeReading, policy =>
                    policy.Requirements.Add(new SectionAliasesRequirement(
                        Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
                        Constants.Applications.Settings, Constants.Applications.Packages)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDocuments, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Content)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessUsers, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Users)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessPartialViews, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.PartialViews)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessPartialViewMacros, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.PartialViewMacros)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessPackages, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Packages)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessLogs, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.LogViewer)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDataTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.DataTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessTemplates, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Templates)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMemberTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.MemberTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessRelationTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.RelationTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.DocumentTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMemberGroups, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.MemberGroups)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMediaTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.MediaTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMacros, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Macros)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessLanguages, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Languages)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Dictionary)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDictionary, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Dictionary, Constants.Trees.Dictionary)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDictionaryOrTemplates, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.Dictionary, Constants.Trees.Templates)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.DocumentTypes, Constants.Trees.Content)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMediaOrMediaTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.MediaTypes, Constants.Trees.Media)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessMembersOrMemberTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.MemberTypes, Constants.Trees.Members)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessAnySchemaTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(Constants.Trees.DataTypes, Constants.Trees.DocumentTypes, Constants.Trees.MediaTypes, Constants.Trees.MemberTypes)));

                options.AddPolicy(AuthorizationPolicies.TreeAccessAnyContentOrTypes, policy =>
                    policy.Requirements.Add(new TreeAliasesRequirement(
                            Constants.Trees.DocumentTypes, Constants.Trees.Content,
                            Constants.Trees.MediaTypes, Constants.Trees.Media,
                            Constants.Trees.MemberTypes, Constants.Trees.Members)));
            });
        }
    }
}
