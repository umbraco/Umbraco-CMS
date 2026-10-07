// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Base class for a <see cref="JsonConverter{T}"/> that delegates part of its (de)serialization of
/// <typeparamref name="T"/> to the default JSON (de)serialization, without recursing back into itself.
/// </summary>
/// <remarks>
/// A converter registered for <typeparamref name="T"/> is invoked for every (de)serialization of that type,
/// including one triggered by this converter itself. Calling <see cref="JsonSerializer"/> with the incoming
/// <see cref="JsonSerializerOptions"/> as-is would therefore recurse infinitely; <see cref="GetOptionsWithoutSelf"/>
/// hands back a clone of those options with every converter that can still convert <typeparamref name="T"/>
/// removed. Removing only this specific instance is not enough: when this converter was produced by a
/// <see cref="System.Text.Json.Serialization.JsonConverterFactory"/> still present in the clone, that factory
/// would simply manufacture an equivalent replacement the next time <typeparamref name="T"/> is resolved.
/// <para>
/// Converter instances are long-lived (typically registered once at startup) and can legitimately be asked to
/// (de)serialize <typeparamref name="T"/> through more than one distinct <see cref="JsonSerializerOptions"/>
/// instance, so the clone is cached per distinct incoming <paramref name="options"/> instance (keyed with a
/// <see cref="ConditionalWeakTable{TKey, TValue}"/>, so an options instance that becomes unreachable doesn't keep
/// its clone alive) rather than assuming there is only ever one.
/// </para>
/// <para>
/// <b>Only pass the returned options to <see cref="JsonSerializer"/>'s public Serialize/Deserialize entry
/// points</b> - never invoke a resolved <see cref="JsonConverter{T}"/>'s Write/Read directly. Built-in converters
/// (collections, reflection-based objects) rely on an ambient write/read stack that only those entry points set
/// up; calling their methods directly skips that setup and can throw. The entry points also mean the returned
/// options apply uniformly to the WHOLE value being (de)serialized, including anything nested inside it - fine
/// for <typeparamref name="T"/> that cannot contain another <typeparamref name="T"/>, but for one that can (e.g.
/// a block that can itself contain another block), a nested occurrence reached this way silently falls back to
/// the default (de)serialization instead of this converter's own; such a converter needs to write its own shape
/// without going through this helper for the properties that could recurse (see <c>BlockItemDataConverter</c>).
/// </para>
/// </remarks>
internal abstract class DelegatingJsonConverterBase<T> : JsonConverter<T>
{
    private readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> _optionsWithoutSelf = new();

    /// <summary>
    /// Gets a clone of <paramref name="options"/> with this converter removed, safe to pass back into
    /// <see cref="JsonSerializer"/> for the default (de)serialization of <typeparamref name="T"/>.
    /// </summary>
    protected JsonSerializerOptions GetOptionsWithoutSelf(JsonSerializerOptions options)
        => _optionsWithoutSelf.GetValue(options, CreateOptionsWithoutSelf);

    private JsonSerializerOptions CreateOptionsWithoutSelf(JsonSerializerOptions options)
    {
        var clone = new JsonSerializerOptions(options);
        for (var i = clone.Converters.Count - 1; i >= 0; i--)
        {
            if (clone.Converters[i].CanConvert(typeof(T)))
            {
                clone.Converters.RemoveAt(i);
            }
        }

        return clone;
    }
}
