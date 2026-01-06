using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// JSON converter for block values, because block value layouts are strongly typed but different from implementation to implementation.
/// </summary>
public class JsonBlockValueConverter : JsonConverter<BlockValue>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsAssignableTo(typeof(BlockValue));

    /// <inheritdoc />
    public override BlockValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start object");
        }

        BlockValue? blockValue;
        try
        {
            blockValue = (BlockValue?)Activator.CreateInstance(typeToConvert);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Unable to create an instance of {nameof(BlockValue)} from type: {typeToConvert.FullName}. Please make sure the type has an default (parameterless) constructor. See the inner exception for more details.", ex);
        }

        if (blockValue is null)
        {
            throw new JsonException($"Could not create an instance of {nameof(BlockValue)} from type: {typeToConvert.FullName}.");
        }

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType is JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                if (propertyName is null)
                {
                    continue;
                }

                switch (propertyName.ToFirstUpperInvariant())
                {
                    case nameof(BlockValue.ContentData):
                        blockValue.ContentData = DeserializeBlockItemData(ref reader, options, typeToConvert, nameof(BlockValue.ContentData));
                        break;
                    case nameof(BlockValue.SettingsData):
                        blockValue.SettingsData = DeserializeBlockItemData(ref reader, options, typeToConvert, nameof(BlockValue.SettingsData));
                        break;
                    case nameof(BlockValue.Layout):
                        DeserializeAndSetLayout(ref reader, options, typeToConvert, blockValue);
                        break;
                    case nameof(BlockValue.Expose):
                        blockValue.Expose = DeserializeBlockVariation(ref reader, options, typeToConvert, nameof(BlockValue.Expose));
                        break;
                }
            }
        }

        return blockValue;
    }

    public override void Write(Utf8JsonWriter writer, BlockValue value, JsonSerializerOptions options)
    {
        value.Layout.TryGetValue(value.PropertyEditorAlias, out IEnumerable<IBlockLayoutItem>? blockLayoutItems);
        blockLayoutItems ??= Enumerable.Empty<IBlockLayoutItem>();

        writer.WriteStartObject();

        writer.WritePropertyName(nameof(BlockValue.ContentData).ToFirstLowerInvariant());
        JsonSerializer.Serialize(writer, value.ContentData, options);

        if (value.SettingsData is not null)
        {
            writer.WritePropertyName(nameof(BlockValue.SettingsData).ToFirstLowerInvariant());
            JsonSerializer.Serialize(writer, value.SettingsData, options);
        }

        writer.WritePropertyName(nameof(BlockValue.Expose).ToFirstLowerInvariant());
        JsonSerializer.Serialize(writer, value.Expose, options);

        Type layoutItemType = GetLayoutItemType(value.GetType());

        writer.WriteStartObject(nameof(BlockValue.Layout));

        if (blockLayoutItems.Any())
        {
            writer.WriteStartArray(value.PropertyEditorAlias);
            foreach (IBlockLayoutItem blockLayoutItem in blockLayoutItems)
            {
                JsonSerializer.Serialize(writer, blockLayoutItem, layoutItemType, options);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();

        writer.WriteEndObject();
    }

    private static Type GetLayoutItemType(Type blockValueType)
    {
        Type? layoutItemType = blockValueType.BaseType?.GenericTypeArguments.FirstOrDefault();
        if (layoutItemType is null || layoutItemType.Implements<IBlockLayoutItem>() is false)
        {
            throw new JsonException($"The {nameof(BlockValue)} implementation should have an {nameof(IBlockLayoutItem)} type as its first generic type argument - found: {layoutItemType?.FullName ?? "none"}.");
        }

        return layoutItemType;
    }

    private List<BlockItemData> DeserializeBlockItemData(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToConvert, string propertyName)
    {
        // Create a copy of the reader in case we have to go into the fallback flow.
        // Given FallbackBlockItemDataDeserialization method will be called following an exception in the main
        // deserialisation flow, the original reader would already advanced past the initial StartArray token and fail on
        // attempting to deserialize it to a JSON array.
        // We can do this as Utf8JsonReader is a struct and thus a value type (see https://stackoverflow.com/questions/71396687/how-to-reset-an-utf8jsonreader-to-its-starting-position).
        Utf8JsonReader readerCopy = reader;
        try
        {
            return CurrentDeserializeBlockItemData(ref reader, options);
        }
        catch (JsonException ex) when (ex.Path?.EndsWith(".values") is true)
        {
            // If we hit a JsonException due to the "values" property conflict, attempt the fallback deserialization (using
            // the copied reader).
            return FallbackBlockItemDataDeserialization(ref readerCopy, options);
        }
    }

    private List<BlockItemData> CurrentDeserializeBlockItemData(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        JsonArray? arrayElement = JsonSerializer.Deserialize<JsonArray>(ref reader, options);
        return arrayElement?
            .Select(itemElement => CurrentDeserializeBlockItemData(itemElement, options))
            .OfType<BlockItemData>()
            .ToList() ?? [];
    }

    private static BlockItemData? CurrentDeserializeBlockItemData(JsonNode? jsonNode, JsonSerializerOptions options)
    {
        if (jsonNode is not JsonObject jsonObject)
        {
            return jsonNode.Deserialize<BlockItemData>(options);
        }

        // Handle legacy "udi" field if present.
        string? udiValue = ExtractLegacyUdi(jsonObject);

        BlockItemData? blockItemData = jsonObject.Deserialize<BlockItemData>(options);

        SetKeyFromLegacyUdi(blockItemData, udiValue);

        return blockItemData;
    }

    /// <summary>
    /// Extracts and removes the legacy "udi" field from a JsonObject if present.
    /// </summary>
    /// <remarks>
    /// The Udi property has [JsonIgnore] to prevent serialization differences between save and publish paths,
    /// so we must handle it manually during deserialization.
    /// </remarks>
    private static string? ExtractLegacyUdi(JsonObject jsonObject)
    {
        if (jsonObject.ContainsKey("udi") is false)
        {
            return null;
        }

        var udiValue = jsonObject["udi"]?.GetValue<string>();
        jsonObject.Remove("udi");
        return udiValue;
    }

    /// <summary>
    /// Sets the Key property from a legacy UDI string if the Key wasn't already set.
    /// </summary>
    private static void SetKeyFromLegacyUdi(BlockItemData? blockItemData, string? udiValue)
    {
        if (blockItemData is null || blockItemData.Key != Guid.Empty || udiValue is null)
        {
            return;
        }

        if (UdiParser.TryParse(udiValue, out Udi? udi) && udi is GuidUdi guidUdi)
        {
            blockItemData.Key = guidUdi.Guid;
        }
    }

    private List<BlockItemVariation> DeserializeBlockVariation(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToConvert, string propertyName)
        => DeserializeListOf<BlockItemVariation>(ref reader, options, typeToConvert, propertyName);

    private List<T> DeserializeListOf<T>(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToConvert, string propertyName)
        => JsonSerializer.Deserialize<List<T>>(ref reader, options)
           ?? throw new JsonException($"Unable to deserialize {propertyName} from type: {typeToConvert.FullName}.");

    private void DeserializeAndSetLayout(ref Utf8JsonReader reader, JsonSerializerOptions options, Type typeToConvert, BlockValue blockValue)
    {
        // the block editor layouts collection can contain layouts from any number of block editors.
        // we only want to deserialize the one identified by the concrete block value.
        // here's an example of how the layouts collection JSON might look:
        //     "layout": {
        //         "Umbraco.BlockGrid": [{
        //                 "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D",
        //                 "rowSpan": 1,
        //                 "columnSpan": 12,
        //                 "areas": []
        //             }
        //         ],
        //         "Umbraco.BlockList": [{
        //                 "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D"
        //             }
        //         ],
        //         "Some.Custom.BlockEditor": [{
        //                 "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D"
        //             }
        //         ]
        //     }

        // the concrete block editor layout items type
        Type layoutItemType = GetLayoutItemType(typeToConvert);
        // the type describing a list of concrete block editor layout items
        Type layoutItemsType = typeof(List<>).MakeGenericType(layoutItemType);

        while (reader.Read())
        {
            if (reader.TokenType is JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType is JsonTokenType.PropertyName)
            {
                // grab the block editor alias (e.g. "Umbraco.BlockGrid")
                var blockEditorAlias = reader.GetString()
                                       ?? throw new JsonException($"Could bot get the block editor alias from the layout while attempting to deserialize type: {typeToConvert.FullName}.");

                // forward the reader to the next JSON token, which *should* be the array of corresponding layout items
                reader.Read();
                if (reader.TokenType is not JsonTokenType.StartArray)
                {
                    throw new JsonException($"Expected to find the beginning of an array of layout items for block editor alias: {blockEditorAlias}, got: {reader.TokenType}. This happened while attempting to deserialize type: {typeToConvert.FullName}.");
                }

                // did we encounter the concrete block value?
                if (blockValue.SupportsBlockLayoutAlias(blockEditorAlias))
                {
                    // yes, deserialize the block layout items as their concrete type (list of layoutItemType)
                    var layoutItems = JsonSerializer.Deserialize(ref reader, layoutItemsType, options);
                    blockValue.Layout[blockValue.PropertyEditorAlias] = layoutItems as IEnumerable<IBlockLayoutItem>
                                                                        ?? throw new JsonException($"Could not deserialize block editor layout items as type: {layoutItemType.FullName} while attempting to deserialize layout items for block editor alias: {blockEditorAlias} for type: {typeToConvert.FullName}.");
                }
                else
                {
                    // ignore this layout - forward the reader to the end of the array and look for the next one

                    // Read past the current StartArray token before we start counting
                    _ = reader.Read();

                    // Keep track of the number of open arrays to ensure we find the correct EndArray token
                    var openCount = 0;
                    while (true)
                    {
                        if (reader.TokenType is JsonTokenType.EndArray && openCount == 0)
                        {
                            break;
                        }

                        if (reader.TokenType is JsonTokenType.StartArray)
                        {
                            openCount++;
                        }
                        else if (reader.TokenType is JsonTokenType.EndArray)
                        {
                            openCount--;
                            if (openCount < 0)
                            {
                                throw new JsonException($"Malformed JSON: Encountered more closing array tokens than opening ones while processing block editor alias: {blockEditorAlias}.");
                            }
                        }

                        if (!reader.Read())
                        {
                            throw new JsonException($"Unexpected end of JSON while looking for the end of the layout items array for block editor alias: {blockEditorAlias}.");
                        }
                    }
                }
            }
        }
    }

    [Obsolete("Only needed to support the old data schema. Remove in V18.")]
    private static List<BlockItemData> FallbackBlockItemDataDeserialization(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        JsonArray? arrayElement = JsonSerializer.Deserialize<JsonArray>(ref reader, options);

        return arrayElement?
            .Select(itemElement => FallbackDeserializeBlockItemData(itemElement, options))
            .OfType<BlockItemData>()
            .ToList() ?? [];
    }

    [Obsolete("Only needed to support the old data schema. Remove in V18.")]
    private static BlockItemData? FallbackDeserializeBlockItemData(JsonNode? jsonNode, JsonSerializerOptions options)
    {
        if (jsonNode is not JsonObject jsonObject || jsonObject.ContainsKey("values") is false)
        {
            // Nothing to be done, just deserialize as usual
            return jsonNode.Deserialize<BlockItemData>(options);
        }

        // Handle legacy "udi" field if present.
        string? udiValue = ExtractLegacyUdi(jsonObject);

        // Handle the "values" property conflict by extracting the "values" property first and adding it to the
        // RawPropertyValues dictionary after deserialization.
        JsonNode? values = jsonObject["values"];
        jsonObject.Remove("values");

        BlockItemData? blockItemData = jsonObject.Deserialize<BlockItemData>(options);
        blockItemData?.RawPropertyValues["values"] = values.Deserialize<object?>(options);

        SetKeyFromLegacyUdi(blockItemData, udiValue);

        return blockItemData;
    }
}
