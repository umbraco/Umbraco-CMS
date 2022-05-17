namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class SqlTemplates
    {
        public static class VersionableRepository
        {
            public const string GetVersionIds = "Umbraco.Core.VersionableRepository.GetVersionIds";
            public const string GetVersion = "Umbraco.Core.VersionableRepository.GetVersion";
            public const string GetVersions = "Umbraco.Core.VersionableRepository.GetVersions";
            public const string EnsureUniqueNodeName = "Umbraco.Core.VersionableRepository.EnsureUniqueNodeName";
            public const string GetSortOrder = "Umbraco.Core.VersionableRepository.GetSortOrder";
            public const string GetParentNode = "Umbraco.Core.VersionableRepository.GetParentNode";
            public const string GetReservedId = "Umbraco.Core.VersionableRepository.GetReservedId";
        }

        public static class RelationRepository
        {
            public const string DeleteByParentAll = "Umbraco.Core.RelationRepository.DeleteByParent";
            public const string DeleteByParentIn = "Umbraco.Core.RelationRepository.DeleteByParentIn";
        }

        public static class DataTypeRepository
        {
            public const string EnsureUniqueNodeName = "Umbraco.Core.DataTypeDefinitionRepository.EnsureUniqueNodeName";
        }

        public static class NuCacheDatabaseDataSource
        {
            public const string WhereNodeId = "Umbraco.Web.PublishedCache.NuCache.DataSource.WhereNodeId";
            public const string WhereNodeIdX = "Umbraco.Web.PublishedCache.NuCache.DataSource.WhereNodeIdX";

            public const string SourcesSelectUmbracoNodeJoin =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.SourcesSelectUmbracoNodeJoin";

            public const string ContentSourcesSelect =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ContentSourcesSelect";

            public const string ContentSourcesCount =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ContentSourcesCount";

            public const string MediaSourcesSelect = "Umbraco.Web.PublishedCache.NuCache.DataSource.MediaSourcesSelect";
            public const string MediaSourcesCount = "Umbraco.Web.PublishedCache.NuCache.DataSource.MediaSourcesCount";

            public const string ObjectTypeNotTrashedFilter =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ObjectTypeNotTrashedFilter";

            public const string OrderByLevelIdSortOrder =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.OrderByLevelIdSortOrder";
        }
    }
}
