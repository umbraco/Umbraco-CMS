namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains SQL template name constants used for parameterized queries.
    /// </summary>
    public static class SqlTemplates
    {
        /// <summary>
        ///     Contains SQL template names for versionable repository operations.
        /// </summary>
        public static class VersionableRepository
        {
            /// <summary>
            ///     The SQL template name for getting version IDs.
            /// </summary>
            public const string GetVersionIds = "Umbraco.Core.VersionableRepository.GetVersionIds";

            /// <summary>
            ///     The SQL template name for getting a single version.
            /// </summary>
            public const string GetVersion = "Umbraco.Core.VersionableRepository.GetVersion";

            /// <summary>
            ///     The SQL template name for getting multiple versions.
            /// </summary>
            public const string GetVersions = "Umbraco.Core.VersionableRepository.GetVersions";

            /// <summary>
            ///     The SQL template name for ensuring unique node names.
            /// </summary>
            public const string EnsureUniqueNodeName = "Umbraco.Core.VersionableRepository.EnsureUniqueNodeName";

            /// <summary>
            ///     The SQL template name for checking if a sort order exists.
            /// </summary>
            public const string SortOrderExists = "Umbraco.Core.VersionableRepository.SortOrderExists";

            /// <summary>
            ///     The SQL template name for getting the sort order.
            /// </summary>
            public const string GetSortOrder = "Umbraco.Core.VersionableRepository.GetSortOrder";

            /// <summary>
            ///     The SQL template name for getting the parent node.
            /// </summary>
            public const string GetParentNode = "Umbraco.Core.VersionableRepository.GetParentNode";

            /// <summary>
            ///     The SQL template name for getting a reserved ID.
            /// </summary>
            public const string GetReservedId = "Umbraco.Core.VersionableRepository.GetReservedId";
        }

        /// <summary>
        ///     Contains SQL template names for relation repository operations.
        /// </summary>
        public static class RelationRepository
        {
            /// <summary>
            ///     The SQL template name for deleting all relations by parent.
            /// </summary>
            public const string DeleteByParentAll = "Umbraco.Core.RelationRepository.DeleteByParent";

            /// <summary>
            ///     The SQL template name for deleting relations by parent using an IN clause.
            /// </summary>
            public const string DeleteByParentIn = "Umbraco.Core.RelationRepository.DeleteByParentIn";
        }

        /// <summary>
        ///     Contains SQL template names for data type repository operations.
        /// </summary>
        public static class DataTypeRepository
        {
            /// <summary>
            ///     The SQL template name for ensuring unique data type node names.
            /// </summary>
            public const string EnsureUniqueNodeName = "Umbraco.Core.DataTypeDefinitionRepository.EnsureUniqueNodeName";
        }

        /// <summary>
        ///     Contains SQL template names for NuCache database data source operations.
        /// </summary>
        public static class NuCacheDatabaseDataSource
        {
            /// <summary>
            ///     The SQL template name for filtering by node ID.
            /// </summary>
            public const string WhereNodeId = "Umbraco.Web.PublishedCache.NuCache.DataSource.WhereNodeId";

            /// <summary>
            ///     The SQL template name for filtering by node key.
            /// </summary>
            public const string WhereNodeKey = "Umbraco.Web.PublishedCache.NuCache.DataSource.WhereNodeKey";

            /// <summary>
            ///     The SQL template name for extended node ID filtering.
            /// </summary>
            public const string WhereNodeIdX = "Umbraco.Web.PublishedCache.NuCache.DataSource.WhereNodeIdX";

            /// <summary>
            ///     The SQL template name for selecting sources with an Umbraco node join.
            /// </summary>
            public const string SourcesSelectUmbracoNodeJoin =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.SourcesSelectUmbracoNodeJoin";

            /// <summary>
            ///     The SQL template name for selecting content sources.
            /// </summary>
            public const string ContentSourcesSelect =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ContentSourcesSelect";

            /// <summary>
            ///     The SQL template name for counting content sources.
            /// </summary>
            public const string ContentSourcesCount =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ContentSourcesCount";

            /// <summary>
            ///     The SQL template name for selecting media sources.
            /// </summary>
            public const string MediaSourcesSelect = "Umbraco.Web.PublishedCache.NuCache.DataSource.MediaSourcesSelect";

            /// <summary>
            ///     The SQL template name for counting media sources.
            /// </summary>
            public const string MediaSourcesCount = "Umbraco.Web.PublishedCache.NuCache.DataSource.MediaSourcesCount";

            /// <summary>
            ///     The SQL template name for filtering by object type excluding trashed items.
            /// </summary>
            public const string ObjectTypeNotTrashedFilter =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.ObjectTypeNotTrashedFilter";

            /// <summary>
            ///     The SQL template name for ordering by level, ID, and sort order.
            /// </summary>
            public const string OrderByLevelIdSortOrder =
                "Umbraco.Web.PublishedCache.NuCache.DataSource.OrderByLevelIdSortOrder";
        }
    }
}
