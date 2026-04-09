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
}
