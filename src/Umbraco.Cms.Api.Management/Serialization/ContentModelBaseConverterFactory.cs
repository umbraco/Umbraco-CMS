// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Produces a <see cref="ContentModelBaseConverter{TConcrete, TValueModel, TVariantModel}"/> for any type that
/// derives - at any depth - from <see cref="ContentModelBase{TValueModel, TVariantModel}"/>, so every such
/// response model gets its <c>Variants</c>/<c>Values</c> ordered consistently without a converter of its own.
/// </summary>
internal sealed class ContentModelBaseConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => TryGetContentModelBaseType(typeToConvert, out _);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (TryGetContentModelBaseType(typeToConvert, out Type? contentModelBaseType) is false)
        {
            throw new NotSupportedException($"{typeToConvert} does not derive from {typeof(ContentModelBase<,>)}.");
        }

        Type[] genericArguments = contentModelBaseType.GetGenericArguments();
        Type converterType = typeof(ContentModelBaseConverter<,,>).MakeGenericType(typeToConvert, genericArguments[0], genericArguments[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private static bool TryGetContentModelBaseType(Type typeToConvert, [NotNullWhen(true)] out Type? contentModelBaseType)
    {
        for (Type? current = typeToConvert; current is not null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ContentModelBase<,>))
            {
                contentModelBaseType = current;
                return true;
            }
        }

        contentModelBaseType = null;
        return false;
    }
}
