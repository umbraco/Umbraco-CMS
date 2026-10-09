// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     Typed configuration options for NuCache settings.
/// </summary>
[UmbracoOptions(Constants.Configuration.ConfigNuCache)]
public class NuCacheSettings
{
    /// <summary>
    ///     The default serializer type for NuCache.
    /// </summary>
    internal const string StaticNuCacheSerializerType = "MessagePack";

    /// <summary>
    ///     The default SQL page size for NuCache queries.
    /// </summary>
    internal const int StaticSqlPageSize = 1000;

    /// <summary>
    ///     The default value for using paged SQL queries.
    /// </summary>
    internal const bool StaticUsePagedSqlQuery = true;

    /// <summary>
    ///     The default number of content items whose stale NuCache rows are deleted per batch during a
    ///     content type rebuild. Matches the SQL parameter limit ceiling the delete is capped to.
    /// </summary>
    internal const int StaticContentTypeRebuildDeleteBatchSize = 2000;

    /// <summary>
    ///     The serializer type that nucache uses to persist documents in the database.
    /// </summary>
    [DefaultValue(StaticNuCacheSerializerType)]
    public NuCacheSerializerType NuCacheSerializerType { get; set; } = Enum.Parse<NuCacheSerializerType>(StaticNuCacheSerializerType);

    /// <summary>
    ///     The paging size to use for nucache SQL queries.
    /// </summary>
    [DefaultValue(StaticSqlPageSize)]
    public int SqlPageSize { get; set; } = StaticSqlPageSize;

    /// <summary>
    ///     Gets or sets a value indicating whether to use paged SQL queries for nucache.
    /// </summary>
    [DefaultValue(StaticUsePagedSqlQuery)]
    public bool UsePagedSqlQuery { get; set; } = true;

    /// <summary>
    ///     Gets or sets the number of content items whose stale NuCache rows are deleted per batch during a
    ///     content type structural rebuild.
    /// </summary>
    /// <remarks>
    ///     Deleting in batches avoids a single unbounded DELETE that can escalate to a table lock, bloat the
    ///     transaction log, or exceed the command timeout on sites with a lot of content. The matching node ids
    ///     are read from the source tables once, then their rows are deleted in batches of this size, so the
    ///     value only bounds the per-statement (and, for a deferred rebuild, per-transaction) footprint — it
    ///     does not cause repeated scans. The effective size is capped at the SQL parameter limit
    ///     (<see cref="Constants.Sql.MaxParameterCount" />); lower it if brief lock escalation on
    ///     <c>cmsContentNu</c> during a rebuild is a concern.
    /// </remarks>
    [DefaultValue(StaticContentTypeRebuildDeleteBatchSize)]
    public int ContentTypeRebuildDeleteBatchSize { get; set; } = StaticContentTypeRebuildDeleteBatchSize;
}
