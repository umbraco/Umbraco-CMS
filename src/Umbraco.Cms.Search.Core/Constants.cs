namespace Umbraco.Cms.Search.Core;

public static class Constants
{
    public static class IndexAliases
    {
        private const string IndexPrefix = "Umb_";

        public const string PublishedContent = $"{IndexPrefix}PublishedContent";

        public const string DraftContent = $"{IndexPrefix}Content";

        public const string DraftMedia = $"{IndexPrefix}Media";

        public const string DraftMembers = $"{IndexPrefix}Members";
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
        public const string IndexDocumentTableName = Umbraco.Cms.Core.Constants.DatabaseSchema.TableNamePrefix + "IndexDocument";
    }
}
