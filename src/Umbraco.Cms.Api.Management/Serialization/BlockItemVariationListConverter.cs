// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Api.Management.Serialization;

internal sealed class BlockItemVariationListConverter : DelegatingJsonConverterBase<IList<BlockItemVariation>>
{
    public override IList<BlockItemVariation>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<IList<BlockItemVariation>>(ref reader, GetOptionsWithoutSelf(options));

    public override void Write(Utf8JsonWriter writer, IList<BlockItemVariation> value, JsonSerializerOptions options)
    {
        IList<BlockItemVariation> ordered = value
            .OrderBy(variation => variation.Culture, StringComparer.Ordinal)
            .ThenBy(variation => variation.Segment, StringComparer.Ordinal)
            .ThenBy(variation => variation.ContentKey)
            .ToList();

        JsonSerializer.Serialize(writer, ordered, GetOptionsWithoutSelf(options));
    }
}
