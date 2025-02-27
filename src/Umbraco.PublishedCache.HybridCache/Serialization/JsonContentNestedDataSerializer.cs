using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.HybridCache.Serialization;

internal class JsonContentNestedDataSerializer : IContentCacheDataSerializer
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public ContentCacheDataModel? Deserialize(
        IReadOnlyContentBase content,
        string? stringData,
        byte[]? byteData,
        bool published)
    {
        if (stringData == null && byteData != null)
        {
            throw new NotSupportedException(
                $"{typeof(JsonContentNestedDataSerializer)} does not support byte[] serialization");
        }

        return JsonSerializer.Deserialize<ContentCacheDataModel>(stringData!, _jsonSerializerOptions);
    }

    /// <inheritdoc />
    public ContentCacheDataSerializationResult Serialize(
        IReadOnlyContentBase content,
        ContentCacheDataModel model,
        bool published)
    {
        var json = JsonSerializer.Serialize(model, _jsonSerializerOptions);
        return new ContentCacheDataSerializationResult(json, null);
    }
}
