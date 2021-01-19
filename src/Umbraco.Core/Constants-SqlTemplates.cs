﻿namespace Umbraco.Core
{
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
        }
    }
}
