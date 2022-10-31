using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.BackOffice.Middleware;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Security;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for <see cref="IUmbracoBuilder" /> for the Umbraco back office
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Adds Umbraco back office authentication requirements
    /// </summary>
    public static IUmbracoBuilder AddBackOfficeAuthentication(this IUmbracoBuilder builder)
    {
        builder.Services

            // This just creates a builder, nothing more
            .AddAuthentication()

            // Add our custom schemes which are cookie handlers
            .AddCookie(Constants.Security.BackOfficeAuthenticationType)
            .AddCookie(Constants.Security.BackOfficeExternalAuthenticationType, o =>
            {
                o.Cookie.Name = Constants.Security.BackOfficeExternalAuthenticationType;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })

            // Although we don't natively support this, we add it anyways so that if end-users implement the required logic
            // they don't have to worry about manually adding this scheme or modifying the sign in manager
            .AddCookie(Constants.Security.BackOfficeTwoFactorAuthenticationType, o =>
            {
                o.Cookie.Name = Constants.Security.BackOfficeTwoFactorAuthenticationType;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddCookie(Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType, o =>
            {
                o.Cookie.Name = Constants.Security.BackOfficeTwoFactorRememberMeAuthenticationType;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        builder.Services.ConfigureOptions<ConfigureBackOfficeCookieOptions>();

        builder.Services.AddSingleton<BackOfficeExternalLoginProviderErrorMiddleware>();

        builder.Services.AddUnique<IBackOfficeAntiforgery, BackOfficeAntiforgery>();
        builder.Services.AddUnique<IPasswordChanger<BackOfficeIdentityUser>, PasswordChanger<BackOfficeIdentityUser>>();
        builder.Services.AddUnique<IPasswordChanger<MemberIdentityUser>, PasswordChanger<MemberIdentityUser>>();

        builder.AddNotificationHandler<UserLoginSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserLogoutSuccessNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserLoginFailedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserForgotPasswordRequestedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserForgotPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserPasswordChangedNotification, BackOfficeUserManagerAuditer>();
        builder.AddNotificationHandler<UserPasswordResetNotification, BackOfficeUserManagerAuditer>();

        return builder;
    }

    /// <summary>
    ///     Adds Umbraco back office authorization policies
    /// </summary>
    public static IUmbracoBuilder AddBackOfficeAuthorizationPolicies(this IUmbracoBuilder builder,
        string backOfficeAuthenticationScheme = Constants.Security.BackOfficeAuthenticationType)
    {
        builder.AddBackOfficeAuthorizationPoliciesInternal(backOfficeAuthenticationScheme);

        builder.Services.AddSingleton<IAuthorizationHandler, FeatureAuthorizeHandler>();

        builder.Services.AddAuthorization(options
            => options.AddPolicy(AuthorizationPolicies.UmbracoFeatureEnabled, policy
                => policy.Requirements.Add(new FeatureAuthorizeRequirement())));

        return builder;
    }

    /// <summary>
    ///     Add authorization handlers and policies
    /// </summary>
    private static void AddBackOfficeAuthorizationPoliciesInternal(this IUmbracoBuilder builder,
        string backOfficeAuthenticationScheme = Constants.Security.BackOfficeAuthenticationType)
    {
        // NOTE: Even though we are registering these handlers globally they will only actually execute their logic for
        // any auth defining a matching requirement and scheme.
        builder.Services.AddSingleton<IAuthorizationHandler, BackOfficeHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TreeHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, SectionHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, AdminUsersHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserGroupHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentPermissionsQueryStringHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentPermissionsResourceHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, ContentPermissionsPublishBranchHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaPermissionsResourceHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, MediaPermissionsQueryStringHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, DenyLocalLoginHandler>();

        builder.Services.AddAuthorization(o => CreatePolicies(o, backOfficeAuthenticationScheme));
    }

    private static void CreatePolicies(AuthorizationOptions options, string backOfficeAuthenticationScheme)
    {
        options.AddPolicy(AuthorizationPolicies.MediaPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new MediaPermissionsResourceRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.MediaPermissionPathById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new MediaPermissionsQueryStringRequirement("id"));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionByResource, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsResourceRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionEmptyRecycleBin, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(Constants.System.RecycleBinContent,
                ActionDelete.ActionLetter));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionAdministrationById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionRights.ActionLetter));
            policy.Requirements.Add(
                new ContentPermissionsQueryStringRequirement(ActionRights.ActionLetter, "contentId"));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionProtectById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionProtect.ActionLetter));
            policy.Requirements.Add(
                new ContentPermissionsQueryStringRequirement(ActionProtect.ActionLetter, "contentId"));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionRollbackById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionRollback.ActionLetter));
            policy.Requirements.Add(
                new ContentPermissionsQueryStringRequirement(ActionRollback.ActionLetter, "contentId"));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionPublishById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionPublish.ActionLetter));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionBrowseById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionBrowse.ActionLetter));
            policy.Requirements.Add(
                new ContentPermissionsQueryStringRequirement(ActionBrowse.ActionLetter, "contentId"));
        });

        options.AddPolicy(AuthorizationPolicies.ContentPermissionDeleteById, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new ContentPermissionsQueryStringRequirement(ActionDelete.ActionLetter));
        });

        options.AddPolicy(AuthorizationPolicies.BackOfficeAccess, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new BackOfficeRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.BackOfficeAccessWithoutApproval, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new BackOfficeRequirement(false));
        });

        options.AddPolicy(AuthorizationPolicies.AdminUserEditsRequireAdmin, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new AdminUsersRequirement());
            policy.Requirements.Add(new AdminUsersRequirement("userIds"));
        });

        options.AddPolicy(AuthorizationPolicies.UserBelongsToUserGroupInRequest, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new UserGroupRequirement());
            policy.Requirements.Add(new UserGroupRequirement("userGroupIds"));
        });

        options.AddPolicy(AuthorizationPolicies.DenyLocalLoginIfConfigured, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new DenyLocalLoginRequirement());
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessContent, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Content));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessContentOrMedia, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(
                new SectionRequirement(Constants.Applications.Content, Constants.Applications.Media));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessUsers, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Users));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessForTinyMce, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(
                Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessMedia, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Media));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessMembers, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Members));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessPackages, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Packages));
        });

        options.AddPolicy(AuthorizationPolicies.SectionAccessSettings, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Settings));
        });

        // We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
        // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
        options.AddPolicy(AuthorizationPolicies.SectionAccessForContentTree, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(
                Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
                Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members));
        });
        options.AddPolicy(AuthorizationPolicies.SectionAccessForMediaTree, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(
                Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Users,
                Constants.Applications.Settings, Constants.Applications.Packages, Constants.Applications.Members));
        });
        options.AddPolicy(AuthorizationPolicies.SectionAccessForMemberTree, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(
                Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members));
        });

        // Permission is granted to this policy if the user has access to any of these sections: Content, media, settings, developer, members
        options.AddPolicy(AuthorizationPolicies.SectionAccessForDataTypeReading, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new SectionRequirement(
                Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members,
                Constants.Applications.Settings, Constants.Applications.Packages));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDocuments, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Content));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessUsers, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Users));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessPartialViews, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.PartialViews));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessPartialViewMacros, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.PartialViewMacros));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessPackages, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Packages));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessLogs, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.LogViewer));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDataTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.DataTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessTemplates, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Templates));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMemberTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.MemberTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessRelationTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.RelationTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.DocumentTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMemberGroups, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.MemberGroups));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMediaTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.MediaTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMacros, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Macros));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessLanguages, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Languages));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDictionary, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Dictionary));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDictionaryOrTemplates, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.Dictionary, Constants.Trees.Templates));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.DocumentTypes, Constants.Trees.Content));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMediaOrMediaTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.MediaTypes, Constants.Trees.Media));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessMembersOrMemberTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.MemberTypes, Constants.Trees.Members));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessAnySchemaTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(Constants.Trees.DataTypes, Constants.Trees.DocumentTypes,
                Constants.Trees.MediaTypes, Constants.Trees.MemberTypes));
        });

        options.AddPolicy(AuthorizationPolicies.TreeAccessAnyContentOrTypes, policy =>
        {
            policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
            policy.Requirements.Add(new TreeRequirement(
                Constants.Trees.DocumentTypes, Constants.Trees.Content,
                Constants.Trees.MediaTypes, Constants.Trees.Media,
                Constants.Trees.MemberTypes, Constants.Trees.Members));
        });
    }
}
