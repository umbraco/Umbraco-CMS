namespace Umbraco.Cms.Web.Common.Authorization;

/// <summary>
///     A list of authorization policy names for use in the back office.
/// </summary>
public static class AuthorizationPolicies
{
    public const string UmbracoFeatureEnabled = nameof(UmbracoFeatureEnabled);

    public const string BackOfficeAccess = nameof(BackOfficeAccess);
    public const string DenyLocalLoginIfConfigured = nameof(DenyLocalLoginIfConfigured);
    public const string RequireAdminAccess = nameof(RequireAdminAccess);
    public const string UserBelongsToUserGroupInRequest = nameof(UserBelongsToUserGroupInRequest);
    public const string UserPermissionByResource = nameof(UserPermissionByResource);

    // Content permission access
    public const string ContentPermissionByResource = nameof(ContentPermissionByResource);

    public const string MediaPermissionByResource = nameof(MediaPermissionByResource);

    // Single section access
    public const string SectionAccessContent = nameof(SectionAccessContent);
    public const string SectionAccessPackages = nameof(SectionAccessPackages);
    public const string SectionAccessUsers = nameof(SectionAccessUsers);
    public const string SectionAccessMedia = nameof(SectionAccessMedia);
    public const string SectionAccessSettings = nameof(SectionAccessSettings);
    public const string SectionAccessMembers = nameof(SectionAccessMembers);

    // Custom access based on multiple sections
    public const string SectionAccessContentOrMedia = nameof(SectionAccessContentOrMedia);
    public const string SectionAccessForMemberTree = nameof(SectionAccessForMemberTree);
    public const string SectionAccessForMediaTree = nameof(SectionAccessForMediaTree);
    public const string SectionAccessForContentTree = nameof(SectionAccessForContentTree);

    // Single tree access
    public const string TreeAccessDocuments = nameof(TreeAccessDocuments);
    public const string TreeAccessPartialViews = nameof(TreeAccessPartialViews);
    public const string TreeAccessDataTypes = nameof(TreeAccessDataTypes);
    public const string TreeAccessWebhooks = nameof(TreeAccessWebhooks);
    public const string TreeAccessTemplates = nameof(TreeAccessTemplates);
    public const string TreeAccessDictionary = nameof(TreeAccessDictionary);
    public const string TreeAccessRelationTypes = nameof(TreeAccessRelationTypes);
    public const string TreeAccessMediaTypes = nameof(TreeAccessMediaTypes);
    public const string TreeAccessLanguages = nameof(TreeAccessLanguages);
    public const string TreeAccessMemberGroups = nameof(TreeAccessMemberGroups);
    public const string TreeAccessDocumentTypes = nameof(TreeAccessDocumentTypes);
    public const string TreeAccessMemberTypes = nameof(TreeAccessMemberTypes);
    public const string TreeAccessScripts = nameof(TreeAccessScripts);
    public const string TreeAccessStylesheets = nameof(TreeAccessStylesheets);

    // Custom access based on multiple trees
    public const string TreeAccessDocumentsOrDocumentTypes = nameof(TreeAccessDocumentsOrDocumentTypes);
    public const string TreeAccessMediaOrMediaTypes = nameof(TreeAccessMediaOrMediaTypes);
    public const string TreeAccessDictionaryOrTemplates = nameof(TreeAccessDictionaryOrTemplates);
    public const string TreeAccessDocumentOrMediaOrContentTypes = nameof(TreeAccessDocumentOrMediaOrContentTypes);
    public const string TreeAccessDocumentsOrMediaOrMembersOrContentTypes = nameof(TreeAccessDocumentsOrMediaOrMembersOrContentTypes);
    public const string TreeAccessStylesheetsOrDocumentOrMediaOrMember = nameof(TreeAccessStylesheetsOrDocumentOrMediaOrMember);
    public const string TreeAccessMembersOrMemberTypes = nameof(TreeAccessMembersOrMemberTypes);

    // other
    public const string DictionaryPermissionByResource = nameof(DictionaryPermissionByResource);
}
