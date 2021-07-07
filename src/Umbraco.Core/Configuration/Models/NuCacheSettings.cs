// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models
{
    /// <summary>
    /// Typed configuration options for NuCache settings.
    /// </summary>
    [UmbracoOptions(Constants.Configuration.ConfigNuCache)]
    public class NuCacheSettings
    {
        internal const string StaticNuCacheSerializerType = "MessagePack";
        internal const int StaticSqlPageSize = 1000;

        /// <summary>
        /// Gets or sets a value defining the BTree block size.
        /// </summary>
        public int? BTreeBlockSize { get; set; }

        /// <summary>
        /// The serializer type that nucache uses to persist documents in the database.
        /// </summary>
        [DefaultValue(StaticNuCacheSerializerType)]
        public NuCacheSerializerType NuCacheSerializerType { get; set; } = Enum<NuCacheSerializerType>.Parse(StaticNuCacheSerializerType);

        /// <summary>
        /// The paging size to use for nucache SQL queries.
        /// </summary>
        [DefaultValue(StaticSqlPageSize)]
        public int SqlPageSize { get; set; } = StaticSqlPageSize;
    }
}
