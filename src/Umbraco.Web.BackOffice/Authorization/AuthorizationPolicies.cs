namespace Umbraco.Web.BackOffice.Authorization
{
    public static class AuthorizationPolicies
    {
        public const string TreeAccessUsers = "TreeAccessUsers";
        public const string TreeAccessPartialViews = "TreeAccessPartialViews";
        public const string TreeAccessPartialViewMacros = "TreeAccessPartialViewMacros";
        public const string TreeAccessDataTypes = "TreeAccessDataTypes";
        public const string TreeAccessPackages = "TreeAccessPackages";
        public const string TreeAccessLogs = "TreeAccessLogs";
        public const string TreeAccessTemplates = "TreeAccessTemplates";
        public const string TreeAccessDictionary = "TreeAccessDictionary";
        public const string TreeAccessRelationTypes = "TreeAccessRelationTypes";
        public const string TreeAccessMediaTypes = "TreeAccessMediaTypes";
        public const string TreeAccessMacros = "TreeAccessMacros";
        public const string TreeAccessLanguages = "TreeAccessLanguages";
        public const string TreeAccessMemberGroups = "TreeAccessMemberGroups";
        public const string TreeAccessDocumentTypes = "TreeAccessDocumentTypes";
        public const string TreeAccessMemberTypes = "TreeAccessMemberTypes";
        public const string TreeAccessDocumentsOrDocumentTypes = "TreeAccessDocumentsAndDocumentTypes";
        public const string TreeAccessMediaOrMediaTypes = "TreeAccessMediaAndMediaTypes";
        public const string TreeAccessMembersOrMemberTypes = "TreeAccessMembersAndMemberTypes";
        public const string TreeAccessAnySchemaTypes = "TreeAccessSchemaTypes";
        public const string TreeAccessDictionaryOrTemplates = "TreeAccessDictionaryOrTemplates";

        /// <summary>
        /// Defines access based on if the user has access to any tree's exposing any types of content (documents, media, members)
        /// or any content types (document types, media types, member types)
        /// </summary>
        public const string TreeAccessAnyContentOrTypes = "TreeAccessAnyContentOrTypes";
    }
}
