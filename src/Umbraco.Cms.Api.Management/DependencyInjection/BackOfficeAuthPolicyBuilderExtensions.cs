using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class BackOfficeAuthPolicyBuilderExtensions
{
    internal static IUmbracoBuilder AddAuthorizationPolicies(this IUmbracoBuilder builder)
    {
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

        options.AddPolicy($"New{AuthorizationPolicies.BackOfficeAccess}", policy =>
        {
            policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
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
        AddPolicy(AuthorizationPolicies.TreeAccessMemberGroups, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Members);
        AddPolicy(AuthorizationPolicies.TreeAccessMemberTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessPartialViews, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessRelationTypes, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessScripts, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessStylesheets, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);
        AddPolicy(AuthorizationPolicies.TreeAccessTemplates, Constants.Security.AllowedApplicationsClaimType, Constants.Applications.Settings);

        AddPolicy(AuthorizationPolicies.RequireAdminAccess, Constants.Security.AllowedApplicationsClaimType, Constants.Security.AdminGroupAlias);
    }
}
