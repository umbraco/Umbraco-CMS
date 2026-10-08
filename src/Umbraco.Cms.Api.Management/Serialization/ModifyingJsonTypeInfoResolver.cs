// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Api.Common.Serialization;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Wraps an <see cref="IUmbracoJsonTypeInfoResolver"/>, applying an additional modifier to every
/// <see cref="JsonTypeInfo"/> it resolves.
/// </summary>
/// <remarks>
/// Lets a single <see cref="JsonSerializerOptions"/> profile layer its own
/// <see cref="JsonTypeInfo.OnSerializing"/>/<see cref="JsonTypeInfo.OnSerialized"/> customization on top of a
/// resolver - such as a shared, injected one - without mutating that resolver itself, which could otherwise leak
/// the customization into every other consumer of the same resolver instance.
/// </remarks>
internal sealed class ModifyingJsonTypeInfoResolver : IUmbracoJsonTypeInfoResolver
{
    private readonly IUmbracoJsonTypeInfoResolver _inner;
    private readonly Action<JsonTypeInfo> _modifier;

    public ModifyingJsonTypeInfoResolver(IUmbracoJsonTypeInfoResolver inner, Action<JsonTypeInfo> modifier)
    {
        _inner = inner;
        _modifier = modifier;
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo? typeInfo = _inner.GetTypeInfo(type, options);
        if (typeInfo is not null)
        {
            _modifier(typeInfo);
        }

        return typeInfo;
    }

    public IEnumerable<Type> FindSubTypes(Type type) => _inner.FindSubTypes(type);

    public string? GetTypeDiscriminatorValue(Type type) => _inner.GetTypeDiscriminatorValue(type);
}
