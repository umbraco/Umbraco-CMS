// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// Orders <see cref="ContentModelBase{TValueModel, TVariantModel}.Variants"/> by (culture, segment) and
/// <see cref="ContentModelBase{TValueModel, TVariantModel}.Values"/> by (culture, segment, alias) for any concrete
/// implementation of <see cref="ContentModelBase{TValueModel, TVariantModel}"/>.
/// </summary>
/// <remarks>
/// Created by <see cref="ContentModelBaseConverterFactory"/> - one instance per concrete type - rather than
/// registered directly, so it is never shared across <see cref="JsonSerializerOptions"/> instances.
/// </remarks>
internal sealed class ContentModelBaseConverter<TConcrete, TValueModel, TVariantModel> : DelegatingJsonConverterBase<TConcrete>
    where TConcrete : ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public override TConcrete? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<TConcrete>(ref reader, GetOptionsWithoutSelf(options));

    public override void Write(Utf8JsonWriter writer, TConcrete value, JsonSerializerOptions options)
    {
        IEnumerable<TVariantModel> originalVariants = value.Variants as TVariantModel[] ?? value.Variants.ToArray();
        IEnumerable<TValueModel> originalValues = value.Values as TValueModel[] ?? value.Values.ToArray();

        try
        {
            value.Variants = originalVariants
                .OrderBy(variant => variant.Culture, StringComparer.Ordinal)
                .ThenBy(variant => variant.Segment, StringComparer.Ordinal)
                .ToArray();
            value.Values = originalValues
                .OrderBy(propertyValue => propertyValue.Culture, StringComparer.Ordinal)
                .ThenBy(propertyValue => propertyValue.Segment, StringComparer.Ordinal)
                .ThenBy(propertyValue => propertyValue.Alias, StringComparer.Ordinal)
                .ToArray();

            JsonSerializer.Serialize(writer, value, GetOptionsWithoutSelf(options));
        }
        finally
        {
            value.Variants = originalVariants;
            value.Values = originalValues;
        }
    }
}
