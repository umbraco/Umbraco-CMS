namespace Umbraco.Cms.Search.Core;

using CoreConstants = Umbraco.Cms.Core.Constants;

/// <summary>
/// Constants used by Umbraco Search.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The aliases of the built-in search indexes.
    /// </summary>
    [Obsolete("Please use CoreConstants.IndexAliases instead. Scheduled for removal in Umbraco 21.")]
    public static class IndexAliases
    {
        /// <summary>
        ///     The alias of the published content index.
        /// </summary>
        public const string PublishedContent = CoreConstants.IndexAliases.PublishedContent;

        /// <summary>
        ///     The alias of the draft content index.
        /// </summary>
        public const string DraftContent = CoreConstants.IndexAliases.DraftContent;

        /// <summary>
        ///     The alias of the draft media index.
        /// </summary>
        public const string DraftMedia = CoreConstants.IndexAliases.DraftMedia;

        /// <summary>
        ///     The alias of the draft members index.
        /// </summary>
        public const string DraftMembers = CoreConstants.IndexAliases.DraftMembers;
    }

    /// <summary>
    /// The names of the system fields written to every index document, all prefixed with <c>Umb_</c>.
    /// </summary>
    public static class FieldNames
    {
        private const string FieldPrefix = "Umb_";

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
    /// API-related constants.
    /// </summary>
    public static class Api
    {
        /// <summary>
        /// The API name used to map the Search Management API endpoints.
        /// </summary>
        public const string Name = "search";
    }

    /// <summary>
    /// Persistence-related constants.
    /// </summary>
    public static class Persistence
    {
        /// <summary>
        /// The name of the database table storing persisted index documents.
        /// </summary>
        public const string IndexDocumentTableName = Umbraco.Cms.Core.Constants.DatabaseSchema.Tables.IndexDocument;
    }
}
