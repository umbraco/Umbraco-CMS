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

        builder.Services.AddAuthorization(CreatePolicies);
        return builder;
    }

    private static void CreatePolicies(AuthorizationOptions options)
    {
        void AddPolicy(string policyName, string claimType, params string[] allowedClaimValues)
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.RequireClaim(claimType, allowedClaimValues);
            });
        }

        options.AddPolicy(AuthorizationPolicies.BackOfficeAccess, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
        });

        options.AddPolicy(AuthorizationPolicies.RequireAdminAccess, policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.RequireRole(Constants.Security.AdminGroupAlias);
        });

        AddPolicy(AuthorizationPolicies.SectionAccessContent, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content);
        AddPolicy(AuthorizationPolicies.SectionAccessContentOrMedia, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content, Constants.Applications.Media);
        AddPolicy(AuthorizationPolicies.SectionAccessForContentTree, Constants.Security.AllowedApplicationsClaimType,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
            Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.SectionAccessForMediaTree, Constants.Security.AllowedApplicationsClaimType,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
            Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.SectionAccessForMemberTree, Constants.Security.AllowedApplicationsClaimType,
            Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.SectionAccessMedia, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Media);
        AddPolicy(AuthorizationPolicies.SectionAccessMembers, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.SectionAccessPackages, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Packages);
        AddPolicy(AuthorizationPolicies.SectionAccessSettings, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.SectionAccessUsers, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Users);

        AddPolicy(AuthorizationPolicies.TreeAccessDataTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessDictionary, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Translation);
        AddPolicy(AuthorizationPolicies.TreeAccessDictionaryOrTemplates, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Translation, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessDocuments, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content);
        AddPolicy(AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Content, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessLanguages, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessMediaTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessMediaOrMediaTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Media, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessMemberGroups, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.TreeAccessMemberTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessPartialViews, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessRelationTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessScripts, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessStylesheets, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessTemplates, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessWebhooks, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);

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
