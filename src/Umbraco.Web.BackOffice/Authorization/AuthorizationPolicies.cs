namespace Umbraco.Web.BackOffice.Authorization
{
    public static class AuthorizationPolicies
    {
        // TODO: Rethink these names! Describe what they are doing probably

        public const string SectionAccessContentOrMedia = nameof(SectionAccessContentOrMedia);
        public const string SectionAccessContent = nameof(SectionAccessContent);
        public const string SectionAccessPackages = nameof(SectionAccessPackages);
        public const string SectionAccessUsers = nameof(SectionAccessUsers);
        public const string SectionAccessMedia = nameof(SectionAccessMedia);
        public const string SectionAccessSettings = nameof(SectionAccessSettings);
        public const string SectionAccessForTinyMce = nameof(SectionAccessForTinyMce);
        public const string SectionAccessForMemberTree = nameof(SectionAccessForMemberTree);
        public const string SectionAccessForMediaTree = nameof(SectionAccessForMediaTree);
        public const string SectionAccessForContentTree = nameof(SectionAccessForContentTree);
        public const string SectionAccessForDataTypeReading = nameof(SectionAccessForDataTypeReading);
        public const string SectionAccessMembers = nameof(SectionAccessMembers);

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
