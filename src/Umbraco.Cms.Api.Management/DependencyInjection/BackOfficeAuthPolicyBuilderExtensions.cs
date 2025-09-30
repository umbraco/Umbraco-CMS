using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.Security.Authorization.DenyLocalLogin;
using Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.Security.Authorization.User;
using Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Authorization;
using FeatureAuthorizeHandler = Umbraco.Cms.Api.Management.Security.Authorization.Feature.FeatureAuthorizeHandler;
using FeatureAuthorizeRequirement = Umbraco.Cms.Api.Management.Security.Authorization.Feature.FeatureAuthorizeRequirement;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class BackOfficeAuthPolicyBuilderExtensions
{
    internal static IUmbracoBuilder AddAuthorizationPolicies(this IUmbracoBuilder builder)
    {
        // NOTE: Even though we are registering these handlers globally they will only actually execute their logic for
        // any auth defining a matching requirement and scheme.
        builder.Services.AddSingleton<IAuthorizationHandler, ContentPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, DenyLocalLoginHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, DictionaryPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserGroupPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, AllowedApplicationHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, BackOfficeHandler>();

        builder.Services.AddAuthorization(CreatePolicies);
        return builder;
    }

    private static void CreatePolicies(AuthorizationOptions options)
    {
        void AddAllowedApplicationsPolicy(string policyName, params string[] allowedClaimValues)
            => options.AddPolicy(policyName, policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.Requirements.Add(new AllowedApplicationRequirement(allowedClaimValues));
            });

        options.AddPolicy(AuthorizationPolicies.BackOfficeAccess, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new BackOfficeRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.RequireAdminAccess, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.RequireRole(Constants.Security.AdminGroupAlias);
        });

        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessContent, Constants.Applications.Content);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessContentOrMedia, Constants.Applications.Content, Constants.Applications.Media);
        AddAllowedApplicationsPolicy(
            AuthorizationPolicies.SectionAccessForContentTree,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
            Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(
            AuthorizationPolicies.SectionAccessForMediaTree,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
            Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(
            AuthorizationPolicies.SectionAccessForMemberTree,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessMedia, Constants.Applications.Media);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessMembers, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessPackages, Constants.Applications.Packages);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessSettings, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.SectionAccessUsers, Constants.Applications.Users);

        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDataTypes, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDictionary, Constants.Applications.Translation);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDictionaryOrTemplates, Constants.Applications.Translation, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDocuments, Constants.Applications.Content);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes, Constants.Applications.Content, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDocumentOrMediaOrContentTypes, Constants.Applications.Content, Constants.Applications.Settings, Constants.Applications.Media);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDocumentsOrMediaOrMembersOrContentTypes, Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessLanguages, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessMediaTypes, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessMediaOrMediaTypes, Constants.Applications.Media, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessMemberGroups, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessMemberTypes, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessMembersOrMemberTypes, Constants.Applications.Settings, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessPartialViews, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessRelationTypes, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessScripts, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessStylesheets, Constants.Applications.Settings);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessStylesheetsOrDocumentOrMediaOrMember, Constants.Applications.Settings, Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessTemplates, Constants.Applications.Settings, Constants.Applications.Content);
        AddAllowedApplicationsPolicy(AuthorizationPolicies.TreeAccessWebhooks, Constants.Applications.Settings);

        // Contextual permissions
        options.AddPolicy(AuthorizationPolicies.ContentPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.DenyLocalLoginIfConfigured, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new DenyLocalLoginRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.DictionaryPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new DictionaryPermissionRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.MediaPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new MediaPermissionRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.UmbracoFeatureEnabled, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new FeatureAuthorizeRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.UserBelongsToUserGroupInRequest, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new UserGroupPermissionRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.UserPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new UserPermissionRequirement());
        });
    }
}
