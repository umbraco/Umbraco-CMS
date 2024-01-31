using System.Text.Json;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

public class SystemTextJsonNestedDataSerializer : IContentCacheDataSerializer
{
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

        return JsonSerializer.Deserialize<ContentCacheDataModel>(stringData!);
    }

    /// <inheritdoc />
    public ContentCacheDataSerializationResult Serialize(
        IReadOnlyContentBase content,
        ContentCacheDataModel model,
        bool published)
    {
        var json = JsonSerializer.Serialize(model);
        return new ContentCacheDataSerializationResult(json, null);
    }
}
