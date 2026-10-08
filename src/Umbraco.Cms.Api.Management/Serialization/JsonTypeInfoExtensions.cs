// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json.Serialization.Metadata;

namespace Umbraco.Cms.Api.Management.Serialization;

internal static class JsonTypeInfoExtensions
{
    /// <summary>
    /// Adds serialization callbacks to a <see cref="JsonTypeInfo"/>, preserving any callbacks already assigned to it.
    /// </summary>
    /// <remarks>
    /// <paramref name="onSerializing"/> runs after any existing <see cref="JsonTypeInfo.OnSerializing"/> callback, and
    /// <paramref name="onSerialized"/> runs before any existing <see cref="JsonTypeInfo.OnSerialized"/> callback, so the
    /// added pair wraps the write as closely as possible.
    /// </remarks>
    public static void AddSerializationCallbacks(this JsonTypeInfo typeInfo, Action<object> onSerializing, Action<object> onSerialized)
    {
        Action<object>? existingOnSerializing = typeInfo.OnSerializing;
        typeInfo.OnSerializing = existingOnSerializing is null
            ? onSerializing
            : obj =>
            {
                existingOnSerializing(obj);
                onSerializing(obj);
            };

        Action<object>? existingOnSerialized = typeInfo.OnSerialized;
        typeInfo.OnSerialized = existingOnSerialized is null
            ? onSerialized
            : obj =>
            {
                onSerialized(obj);
                existingOnSerialized(obj);
            };
    }
}
