namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains constants for Umbraco Search.
    /// </summary>
    public static class Search
    {
        private const string FieldPrefix = "Umb_";

        /// <summary>
        ///     Contains constants for Umbraco Search index aliases.
        /// </summary>
        public static class IndexAliases
        {
            /// <summary>
            ///     The alias of the published content index, used as the default index for template searches via
            ///     <see cref="IPublishedContentQuery" />.
            /// </summary>
            public const string PublishedContent = $"{IndexPrefix}PublishedContent";

            /// <summary>
            ///     The alias of the draft content index.
            /// </summary>
            public const string DraftContent = $"{IndexPrefix}Content";

            /// <summary>
            ///     The alias of the draft media index.
            /// </summary>
            public const string DraftMedia = $"{IndexPrefix}Media";

            /// <summary>
            ///     The alias of the draft members index.
            /// </summary>
            public const string DraftMembers = $"{IndexPrefix}Members";

            /// <summary>
            ///     The alias of the draft elements index.
            /// </summary>
            public const string DraftElements = $"{IndexPrefix}Elements";

            private const string IndexPrefix = "Umb_";
        }

        /// <summary>
        /// The names of the system fields written to every search index document, all prefixed with <c>Umb_</c>.
        /// </summary>
        public static class FieldNames
        {
            /// <summary>
            /// The field name for the entity ID.
            /// </summary>
            public const string Id = $"{FieldPrefix}Id";

            /// <summary>
            /// The field name for the parent entity ID.
            /// </summary>
            public const string ParentId = $"{FieldPrefix}ParentId";

            /// <summary>
            /// The field name for the ancestor-or-self path IDs.
            /// </summary>
            public const string PathIds = $"{FieldPrefix}PathIds";

            /// <summary>
            /// The field name for the entity name.
            /// </summary>
            public const string Name = $"{FieldPrefix}Name";

            /// <summary>
            /// The field name for the content type ID.
            /// </summary>
            public const string ContentTypeId = $"{FieldPrefix}ContentTypeId";

            /// <summary>
            /// The field name for the creation date.
            /// </summary>
            public const string CreateDate = $"{FieldPrefix}CreateDate";

            /// <summary>
            /// The field name for the last update date.
            /// </summary>
            public const string UpdateDate = $"{FieldPrefix}UpdateDate";

            /// <summary>
            /// The field name for the tree level.
            /// </summary>
            public const string Level = $"{FieldPrefix}Level";

            /// <summary>
            /// The field name for the sort order.
            /// </summary>
            public const string SortOrder = $"{FieldPrefix}SortOrder";

            /// <summary>
            /// The field name for the Umbraco object type.
            /// </summary>
            public const string ObjectType = $"{FieldPrefix}ObjectType";

            /// <summary>
            /// The field name for accumulated tags.
            /// </summary>
            public const string Tags = $"{FieldPrefix}Tags";
        }

        /// <summary>
        /// The names of the system fields written to every member search index document, all prefixed with <c>Umb_</c>.
        /// </summary>
        /// <remarks>
        /// These are written for content-based members and external members alike.
        /// </remarks>
        public static class MemberFieldNames
        {
            /// <summary>
            /// The field name for the member's email address.
            /// </summary>
            public const string Email = $"{FieldPrefix}Email";

            /// <summary>
            /// The field name for the member's username.
            /// </summary>
            public const string UserName = $"{FieldPrefix}UserName";

            /// <summary>
            /// The field name indicating whether the member is approved.
            /// </summary>
            public const string IsApproved = $"{FieldPrefix}IsApproved";

            /// <summary>
            /// The field name indicating whether the member is locked out.
            /// </summary>
            public const string IsLockedOut = $"{FieldPrefix}IsLockedOut";

            /// <summary>
            /// The field name indicating whether the member is an external member rather than a content-based member.
            /// </summary>
            public const string IsExternalMember = $"{FieldPrefix}IsExternalMember";
        }
    }
}
