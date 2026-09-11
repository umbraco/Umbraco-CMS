// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Api.Management.Serialization;

public class BlockItemDataConverter : DelegatingJsonConverterBase<BlockItemData>
{
    public override BlockItemData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<BlockItemData>(ref reader, GetOptionsWithoutSelf(options));

    public override void Write(Utf8JsonWriter writer, BlockItemData value, JsonSerializerOptions options)
    {
        value.Values = value.Values
            .OrderBy(propertyValue => propertyValue.Culture, StringComparer.Ordinal)
            .ThenBy(propertyValue => propertyValue.Segment, StringComparer.Ordinal)
            .ThenBy(propertyValue => propertyValue.Alias, StringComparer.Ordinal)
            .ToList();

        JsonSerializer.Serialize(writer, value, GetOptionsWithoutSelf(options));
    }
}
