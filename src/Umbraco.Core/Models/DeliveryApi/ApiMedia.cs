namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class ApiMedia : IApiMedia
{
    public ApiMedia(Guid id, string name, string mediaType, string url, string? extension, int? width, int? height, int? bytes, IDictionary<string, object?> properties)
    {
        Id = id;
        Name = name;
        MediaType = mediaType;
        Url = url;
        Extension = extension;
        Width = width;
        Height = height;
        Bytes = bytes;
        Properties = properties;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string MediaType { get; }

    public string Url { get; }

    public string? Extension { get; }

    public int? Width { get; }

    public int? Height { get; }

    public int? Bytes { get; }

    public IDictionary<string, object?> Properties { get; }
}
