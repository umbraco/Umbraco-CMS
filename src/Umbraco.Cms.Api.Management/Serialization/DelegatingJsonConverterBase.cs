// Copyright (c) Umbraco.
// See LICENSE for more details.

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
/// This assumes a converter instance is only ever used with a single, stable <see cref="JsonSerializerOptions"/>
/// instance for its lifetime, which holds as long as it is registered once (e.g. via a JSON options configuration
/// callback). Reusing the same converter instance across a different options instance throws, rather than silently
/// returning a clone derived from the wrong options.
/// </para>
/// </remarks>
internal abstract class DelegatingJsonConverterBase<T> : JsonConverter<T>
{
    private readonly Lock _lock = new();
    private JsonSerializerOptions? _sourceOptions;
    private JsonSerializerOptions? _optionsWithoutSelf;

    /// <summary>
    /// Gets a clone of <paramref name="options"/> with this converter removed, safe to pass back into
    /// <see cref="JsonSerializer"/> for the default (de)serialization of <typeparamref name="T"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This converter instance was previously used with a different <see cref="JsonSerializerOptions"/> instance.
    /// </exception>
    protected JsonSerializerOptions GetOptionsWithoutSelf(JsonSerializerOptions options)
    {
        lock (_lock)
        {
            if (_optionsWithoutSelf is not null)
            {
                if (!ReferenceEquals(_sourceOptions, options))
                {
                    throw new InvalidOperationException(
                        $"{GetType().Name} was invoked with a different {nameof(JsonSerializerOptions)} instance than it was first used with. "
                        + "This converter assumes it is registered on exactly one, stable options instance.");
                }

                return _optionsWithoutSelf;
            }

            var clone = new JsonSerializerOptions(options);
            for (var i = clone.Converters.Count - 1; i >= 0; i--)
            {
                if (clone.Converters[i].CanConvert(typeof(T)))
                {
                    clone.Converters.RemoveAt(i);
                }
            }

            _sourceOptions = options;
            _optionsWithoutSelf = clone;
            return clone;
        }
    }
}
