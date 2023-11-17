using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Api.Management.Security.Authorization.BackOffice;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.Security.Authorization.Content.Branch;
using Umbraco.Cms.Api.Management.Security.Authorization.Content.RecycleBin;
using Umbraco.Cms.Api.Management.Security.Authorization.Content.Root;
using Umbraco.Cms.Api.Management.Security.Authorization.DenyLocalLogin;
using Umbraco.Cms.Api.Management.Security.Authorization.Feature;
using Umbraco.Cms.Api.Management.Security.Authorization.Media;
using Umbraco.Cms.Api.Management.Security.Authorization.Media.RecycleBin;
using Umbraco.Cms.Api.Management.Security.Authorization.Media.Root;
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
        builder.Services.AddSingleton<IAuthorizationHandler, BackOfficePermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentBranchPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentRecycleBinPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentRootPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, DenyLocalLoginHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaRecycleBinPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaRootPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserGroupPermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserPermissionHandler>();

        builder.Services.AddSingleton<IAuthorizationHelper, AuthorizationHelper>();
        builder.Services.AddSingleton<IBackOfficePermissionAuthorizer, BackOfficePermissionAuthorizer>();
        builder.Services.AddSingleton<IContentPermissionAuthorizer, ContentPermissionAuthorizer>();
        builder.Services.AddSingleton<IFeatureAuthorizer, FeatureAuthorizer>();
        builder.Services.AddSingleton<IMediaPermissionAuthorizer, MediaPermissionAuthorizer>();
        builder.Services.AddSingleton<IUserGroupPermissionAuthorizer, UserGroupPermissionAuthorizer>();
        builder.Services.AddSingleton<IUserPermissionAuthorizer, UserPermissionAuthorizer>();

        builder.Services.AddAuthorization(CreatePolicies);
        return builder;
    }

    private static void CreatePolicies(AuthorizationOptions options)
    {
        void AddPolicy(string policyName, string claimType, params string[] allowedClaimValues)
        {
            options.AddPolicy($"New{policyName}", policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.RequireClaim(claimType, allowedClaimValues);
            });
        }

        options.AddPolicy($"New{AuthorizationPolicies.RequireAdminAccess}", policy =>
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

        options.AddPolicy($"New{AuthorizationPolicies.BackOfficeAccess}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new BackOfficePermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.BackOfficeAccessWithoutApproval}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new BackOfficePermissionRequirement(false));
        });

        // Contextual permissions
        // TODO: Rename policies once we have the old ones removed
        options.AddPolicy($"New{AuthorizationPolicies.AdminUserEditsRequireAdmin}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new UserPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.ContentPermissionAtRoot}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new ContentRootPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.ContentPermissionByResource}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.ContentPermissionEmptyRecycleBin}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new ContentRecycleBinPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.DenyLocalLoginIfConfigured}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new DenyLocalLoginRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.MediaPermissionAtRoot}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new MediaRootPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.MediaPermissionByResource}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new MediaPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.MediaPermissionEmptyRecycleBin}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new MediaRecycleBinPermissionRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.UmbracoFeatureEnabled}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new FeatureAuthorizeRequirement());
        });

        options.AddPolicy($"New{AuthorizationPolicies.UserBelongsToUserGroupInRequest}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.Requirements.Add(new UserGroupPermissionRequirement());
        });
    }
}
