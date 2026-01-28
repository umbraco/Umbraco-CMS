namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents media in the Delivery API.
/// </summary>
public sealed class ApiMedia : IApiMedia
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiMedia" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the media.</param>
    /// <param name="name">The name of the media.</param>
    /// <param name="mediaType">The media type alias.</param>
    /// <param name="url">The URL of the media.</param>
    /// <param name="extension">The file extension of the media.</param>
    /// <param name="width">The width of the media in pixels.</param>
    /// <param name="height">The height of the media in pixels.</param>
    /// <param name="bytes">The size of the media in bytes.</param>
    /// <param name="properties">The property values of the media.</param>
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

    /// <inheritdoc />
    public Guid Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string MediaType { get; }

    /// <inheritdoc />
    public string Url { get; }

    /// <inheritdoc />
    public string? Extension { get; }

    /// <inheritdoc />
    public int? Width { get; }

    /// <inheritdoc />
    public int? Height { get; }

    /// <inheritdoc />
    public int? Bytes { get; }

    /// <inheritdoc />
    public IDictionary<string, object?> Properties { get; }
}
