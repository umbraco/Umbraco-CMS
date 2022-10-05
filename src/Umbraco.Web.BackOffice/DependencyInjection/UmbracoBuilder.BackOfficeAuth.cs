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

        builder.Services.AddAuthorization(o =>
        {
            CreatePolicies(o, backOfficeAuthenticationScheme);
        });
    }

    // TODO: create the correct policies for new backoffice auth
    private static void CreatePolicies(AuthorizationOptions options, string backOfficeAuthenticationScheme)
    {
        void AddPolicy(string policyName, params IAuthorizationRequirement[] requirements)
        {
            options.AddPolicy(policyName, policy =>
            {
                policy.AuthenticationSchemes.Add(backOfficeAuthenticationScheme);
                // TODO: use constant OpenIddictServerAspNetCoreDefaults.AuthenticationScheme instead of magic string
                policy.AuthenticationSchemes.Add("OpenIddict.Validation.AspNetCore");
                foreach (IAuthorizationRequirement requirement in requirements)
                {
                    policy.Requirements.Add(requirement);
                }
            });
        }

        AddPolicy(AuthorizationPolicies.MediaPermissionByResource, new MediaPermissionsResourceRequirement());

        AddPolicy(AuthorizationPolicies.MediaPermissionPathById, new MediaPermissionsQueryStringRequirement("id"));

        AddPolicy(AuthorizationPolicies.ContentPermissionByResource, new ContentPermissionsResourceRequirement());

        AddPolicy(
            AuthorizationPolicies.ContentPermissionEmptyRecycleBin,
            new ContentPermissionsQueryStringRequirement(Constants.System.RecycleBinContent, ActionDelete.ActionLetter));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionAdministrationById,
            new ContentPermissionsQueryStringRequirement(ActionRights.ActionLetter),
            new ContentPermissionsQueryStringRequirement(ActionRights.ActionLetter, "contentId"));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionProtectById,
            new ContentPermissionsQueryStringRequirement(ActionProtect.ActionLetter),
            new ContentPermissionsQueryStringRequirement(ActionProtect.ActionLetter, "contentId"));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionRollbackById,
            new ContentPermissionsQueryStringRequirement(ActionRollback.ActionLetter),
            new ContentPermissionsQueryStringRequirement(ActionRollback.ActionLetter, "contentId"));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionPublishById,
            new ContentPermissionsQueryStringRequirement(ActionPublish.ActionLetter));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionBrowseById,
            new ContentPermissionsQueryStringRequirement(ActionBrowse.ActionLetter),
            new ContentPermissionsQueryStringRequirement(ActionBrowse.ActionLetter, "contentId"));

        AddPolicy(
            AuthorizationPolicies.ContentPermissionDeleteById,
            new ContentPermissionsQueryStringRequirement(ActionDelete.ActionLetter));

        AddPolicy(AuthorizationPolicies.BackOfficeAccess, new BackOfficeRequirement());

        AddPolicy(AuthorizationPolicies.BackOfficeAccessWithoutApproval, new BackOfficeRequirement(false));

        AddPolicy(
            AuthorizationPolicies.AdminUserEditsRequireAdmin,
            new AdminUsersRequirement(),
            new AdminUsersRequirement("userIds"));

        AddPolicy(
            AuthorizationPolicies.UserBelongsToUserGroupInRequest,
            new UserGroupRequirement(),
            new UserGroupRequirement("userGroupIds"));

        AddPolicy(AuthorizationPolicies.DenyLocalLoginIfConfigured, new DenyLocalLoginRequirement());

        AddPolicy(AuthorizationPolicies.SectionAccessContent, new SectionRequirement(Constants.Applications.Content));

        AddPolicy(
            AuthorizationPolicies.SectionAccessContentOrMedia,
            new SectionRequirement(Constants.Applications.Content, Constants.Applications.Media));

        AddPolicy(AuthorizationPolicies.SectionAccessUsers, new SectionRequirement(Constants.Applications.Users));

        AddPolicy(
            AuthorizationPolicies.SectionAccessForTinyMce,
            new SectionRequirement(Constants.Applications.Content, Constants.Applications.Media, Constants.Applications.Members));

        AddPolicy(AuthorizationPolicies.SectionAccessMedia, new SectionRequirement(Constants.Applications.Media));

        AddPolicy(AuthorizationPolicies.SectionAccessMembers, new SectionRequirement(Constants.Applications.Members));

        AddPolicy(AuthorizationPolicies.SectionAccessPackages, new SectionRequirement(Constants.Applications.Packages));

        AddPolicy(AuthorizationPolicies.SectionAccessSettings, new SectionRequirement(Constants.Applications.Settings));

        // We will not allow the tree to render unless the user has access to any of the sections that the tree gets rendered
        // this is not ideal but until we change permissions to be tree based (not section) there's not much else we can do here.
        AddPolicy(
            AuthorizationPolicies.SectionAccessForContentTree,
            new SectionRequirement(
                Constants.Applications.Content,
                Constants.Applications.Media,
                Constants.Applications.Users,
                Constants.Applications.Settings,
                Constants.Applications.Packages,
                Constants.Applications.Members));

        AddPolicy(
            AuthorizationPolicies.SectionAccessForMediaTree,
            new SectionRequirement(
                Constants.Applications.Content,
                Constants.Applications.Media,
                Constants.Applications.Users,
                Constants.Applications.Settings,
                Constants.Applications.Packages,
                Constants.Applications.Members));

        AddPolicy(
            AuthorizationPolicies.SectionAccessForMemberTree,
            new SectionRequirement(
                Constants.Applications.Content,
                Constants.Applications.Media,
                Constants.Applications.Members));

        // Permission is granted to this policy if the user has access to any of these sections: Content, media, settings, developer, members
        AddPolicy(
            AuthorizationPolicies.SectionAccessForDataTypeReading,
            new SectionRequirement(
                Constants.Applications.Content,
                Constants.Applications.Media,
                Constants.Applications.Members,
                Constants.Applications.Settings,
                Constants.Applications.Packages));

        AddPolicy(AuthorizationPolicies.TreeAccessDocuments, new TreeRequirement(Constants.Trees.Content));

        AddPolicy(AuthorizationPolicies.TreeAccessUsers, new TreeRequirement(Constants.Trees.Users));

        AddPolicy(AuthorizationPolicies.TreeAccessPartialViews, new TreeRequirement(Constants.Trees.PartialViews));

        AddPolicy(AuthorizationPolicies.TreeAccessPartialViewMacros, new TreeRequirement(Constants.Trees.PartialViewMacros));

        AddPolicy(AuthorizationPolicies.TreeAccessPackages, new TreeRequirement(Constants.Trees.Packages));

        AddPolicy(AuthorizationPolicies.TreeAccessLogs, new TreeRequirement(Constants.Trees.LogViewer));

        AddPolicy(AuthorizationPolicies.TreeAccessDataTypes, new TreeRequirement(Constants.Trees.DataTypes));

        AddPolicy(AuthorizationPolicies.TreeAccessTemplates, new TreeRequirement(Constants.Trees.Templates));

        AddPolicy(AuthorizationPolicies.TreeAccessMemberTypes, new TreeRequirement(Constants.Trees.MemberTypes));

        AddPolicy(AuthorizationPolicies.TreeAccessRelationTypes, new TreeRequirement(Constants.Trees.RelationTypes));

        AddPolicy(AuthorizationPolicies.TreeAccessDocumentTypes, new TreeRequirement(Constants.Trees.DocumentTypes));

        AddPolicy(AuthorizationPolicies.TreeAccessMemberGroups, new TreeRequirement(Constants.Trees.MemberGroups));

        AddPolicy(AuthorizationPolicies.TreeAccessMediaTypes, new TreeRequirement(Constants.Trees.MediaTypes));

        AddPolicy(AuthorizationPolicies.TreeAccessMacros, new TreeRequirement(Constants.Trees.Macros));

        AddPolicy(AuthorizationPolicies.TreeAccessLanguages, new TreeRequirement(Constants.Trees.Languages));

        AddPolicy(AuthorizationPolicies.TreeAccessDictionary, new TreeRequirement(Constants.Trees.Dictionary));

        AddPolicy(
            AuthorizationPolicies.TreeAccessDictionaryOrTemplates,
            new TreeRequirement(Constants.Trees.Dictionary, Constants.Trees.Templates));

        AddPolicy(
            AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes,
            new TreeRequirement(Constants.Trees.DocumentTypes, Constants.Trees.Content));

        AddPolicy(
            AuthorizationPolicies.TreeAccessMediaOrMediaTypes,
            new TreeRequirement(Constants.Trees.MediaTypes, Constants.Trees.Media));

        AddPolicy(
            AuthorizationPolicies.TreeAccessMembersOrMemberTypes,
            new TreeRequirement(Constants.Trees.MemberTypes, Constants.Trees.Members));

        AddPolicy(
            AuthorizationPolicies.TreeAccessAnySchemaTypes,
            new TreeRequirement(
                Constants.Trees.DataTypes,
                Constants.Trees.DocumentTypes,
                Constants.Trees.MediaTypes,
                Constants.Trees.MemberTypes));

        AddPolicy(
            AuthorizationPolicies.TreeAccessAnyContentOrTypes,
            new TreeRequirement(
                Constants.Trees.DocumentTypes,
                Constants.Trees.Content,
                Constants.Trees.MediaTypes,
                Constants.Trees.Media,
                Constants.Trees.MemberTypes,
                Constants.Trees.Members));
    }
}
