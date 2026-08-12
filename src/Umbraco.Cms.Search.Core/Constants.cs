namespace Umbraco.Cms.Search.Core;

using CoreConstants = Umbraco.Cms.Core.Constants;

public static class Constants
{
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

    public static class FieldNames
    {
        private const string FieldPrefix = "Umb_";

        public const string Id = $"{FieldPrefix}Id";

        public const string ParentId = $"{FieldPrefix}ParentId";

        public const string PathIds = $"{FieldPrefix}PathIds";

        public const string Name = $"{FieldPrefix}Name";

        public const string ContentTypeId = $"{FieldPrefix}ContentTypeId";

        public const string CreateDate = $"{FieldPrefix}CreateDate";

        public const string UpdateDate = $"{FieldPrefix}UpdateDate";

        public const string Level = $"{FieldPrefix}Level";

        public const string SortOrder = $"{FieldPrefix}SortOrder";

        public const string ObjectType = $"{FieldPrefix}ObjectType";

        public const string Tags = $"{FieldPrefix}Tags";
    }

    public static class Api
    {
        public const string Name = "search";
    }

    public static class Persistence
    {
        public const string IndexDocumentTableName = Umbraco.Cms.Core.Constants.DatabaseSchema.Tables.IndexDocument;
    }
}
