// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
///     The serializer type that the published content cache uses to persist documents in the database.
/// </summary>
public enum NuCacheSerializerType
{
    /// <summary>
    /// The default serializer type, which uses MessagePack for serialization.
    /// </summary>
    MessagePack = 1,

    /// <summary>
    /// The legacy JSON serializer type, which uses JSON for serialization.
    /// </summary>
    /// <remarks>
    /// This option was provided for backward compatibility for the Umbraco cache implementation used from Umbraco 8 to 14 (NuCache).
    /// It is no longer supported with the cache implementation from Umbraco 15 based on .NET's Hybrid cache.
    /// Use the faster and more compact <see cref="MessagePack"/> instead.
    /// The option is kept available only for a more readable format suitable for testing purposes.
    /// </remarks>
    JSON = 2,
}
