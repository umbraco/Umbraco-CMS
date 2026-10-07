// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <remarks>
/// A block's own property values can themselves hold another <see cref="BlockItemData"/> (e.g. a nested Block
/// List), reached through the <c>object</c>-typed <see cref="BlockPropertyValue.Value"/>. <see cref="Write"/>
/// therefore writes <see cref="BlockItemData"/>'s own shape by hand instead of going through
/// <see cref="DelegatingJsonConverterBase{T}.GetOptionsWithoutSelf"/>: that helper's cloned options would apply
/// to the WHOLE value being written, including any nested <see cref="BlockItemData"/>, silently losing this
/// converter's sorting for it. Writing the shape directly keeps the real <paramref name="options"/> - this
/// converter included - in play for <see cref="BlockItemData.Values"/>, so a nested occurrence is reached as a
/// distinct value and sorted the same way.
/// </remarks>
internal sealed class BlockItemDataConverter : DelegatingJsonConverterBase<BlockItemData>
{
    public override BlockItemData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<BlockItemData>(ref reader, GetOptionsWithoutSelf(options));

    public override void Write(Utf8JsonWriter writer, BlockItemData value, JsonSerializerOptions options)
    {
        IList<BlockPropertyValue> sortedValues = value.Values
            .OrderBy(propertyValue => propertyValue.Culture, StringComparer.Ordinal)
            .ThenBy(propertyValue => propertyValue.Segment, StringComparer.Ordinal)
            .ThenBy(propertyValue => propertyValue.Alias, StringComparer.Ordinal)
            .ToList();

        writer.WriteStartObject();

        writer.WritePropertyName(ConvertName(nameof(BlockItemData.ContentTypeKey), options));
        writer.WriteStringValue(value.ContentTypeKey);

        writer.WritePropertyName(ConvertName(nameof(BlockItemData.Key), options));
        writer.WriteStringValue(value.Key);

        writer.WritePropertyName(ConvertName(nameof(BlockItemData.Values), options));
        JsonSerializer.Serialize(writer, sortedValues, options);

        writer.WriteEndObject();
    }

    private static string ConvertName(string name, JsonSerializerOptions options)
        => options.PropertyNamingPolicy?.ConvertName(name) ?? name;
}
