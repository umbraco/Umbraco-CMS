namespace Umbraco.Web.Common.Authorization
{
    /// <summary>
    /// A list of authorization policy names for use in the back office
    /// </summary>
    public static class AuthorizationPolicies
    {
        public const string UmbracoFeatureEnabled = nameof(UmbracoFeatureEnabled);

        public const string BackOfficeAccess = nameof(BackOfficeAccess);
        public const string BackOfficeAccessWithoutApproval = nameof(BackOfficeAccessWithoutApproval);
        public const string UserBelongsToUserGroupInRequest = nameof(UserBelongsToUserGroupInRequest);
        public const string AdminUserEditsRequireAdmin = nameof(AdminUserEditsRequireAdmin);
        public const string DenyLocalLoginIfConfigured = nameof(DenyLocalLoginIfConfigured);

        // Single section access

        public const string SectionAccessContent = nameof(SectionAccessContent);
        public const string SectionAccessPackages = nameof(SectionAccessPackages);
        public const string SectionAccessUsers = nameof(SectionAccessUsers);
        public const string SectionAccessMedia = nameof(SectionAccessMedia);
        public const string SectionAccessSettings = nameof(SectionAccessSettings);
        public const string SectionAccessMembers = nameof(SectionAccessMembers);

        // Custom access based on multiple sections

        public const string SectionAccessContentOrMedia = nameof(SectionAccessContentOrMedia);
        public const string SectionAccessForTinyMce = nameof(SectionAccessForTinyMce);
        public const string SectionAccessForMemberTree = nameof(SectionAccessForMemberTree);
        public const string SectionAccessForMediaTree = nameof(SectionAccessForMediaTree);
        public const string SectionAccessForContentTree = nameof(SectionAccessForContentTree);
        public const string SectionAccessForDataTypeReading = nameof(SectionAccessForDataTypeReading);

        // Single tree access

        public const string TreeAccessDocuments = nameof(TreeAccessDocuments);
        public const string TreeAccessUsers = nameof(TreeAccessUsers);
        public const string TreeAccessPartialViews = nameof(TreeAccessPartialViews);
        public const string TreeAccessPartialViewMacros = nameof(TreeAccessPartialViewMacros);
        public const string TreeAccessDataTypes = nameof(TreeAccessDataTypes);
        public const string TreeAccessPackages = nameof(TreeAccessPackages);
        public const string TreeAccessLogs = nameof(TreeAccessLogs);
        public const string TreeAccessTemplates = nameof(TreeAccessTemplates);
        public const string TreeAccessDictionary = nameof(TreeAccessDictionary);
        public const string TreeAccessRelationTypes = nameof(TreeAccessRelationTypes);
        public const string TreeAccessMediaTypes = nameof(TreeAccessMediaTypes);
        public const string TreeAccessMacros = nameof(TreeAccessMacros);
        public const string TreeAccessLanguages = nameof(TreeAccessLanguages);
        public const string TreeAccessMemberGroups = nameof(TreeAccessMemberGroups);
        public const string TreeAccessDocumentTypes = nameof(TreeAccessDocumentTypes);
        public const string TreeAccessMemberTypes = nameof(TreeAccessMemberTypes);

        // Custom access based on multiple trees

        public const string TreeAccessDocumentsOrDocumentTypes = nameof(TreeAccessDocumentsOrDocumentTypes);
        public const string TreeAccessMediaOrMediaTypes = nameof(TreeAccessMediaOrMediaTypes);
        public const string TreeAccessMembersOrMemberTypes = nameof(TreeAccessMembersOrMemberTypes);
        public const string TreeAccessAnySchemaTypes = nameof(TreeAccessAnySchemaTypes);
        public const string TreeAccessDictionaryOrTemplates = nameof(TreeAccessDictionaryOrTemplates);

        /// <summary>
        /// Defines access based on if the user has access to any tree's exposing any types of content (documents, media, members)
        /// or any content types (document types, media types, member types)
        /// </summary>
        public const string TreeAccessAnyContentOrTypes = nameof(TreeAccessAnyContentOrTypes);
    }
}
