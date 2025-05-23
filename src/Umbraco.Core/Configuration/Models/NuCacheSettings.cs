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
    internal const string StaticNuCacheSerializerType = "MessagePack";
    internal const int StaticSqlPageSize = 1000;
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

    [DefaultValue(StaticUsePagedSqlQuery)]
    public bool UsePagedSqlQuery { get; set; } = true;
}
