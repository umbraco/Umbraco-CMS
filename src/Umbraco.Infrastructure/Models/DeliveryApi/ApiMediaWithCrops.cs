namespace Umbraco.Cms.Core.Models.DeliveryApi;

internal class ApiMediaWithCrops : IApiMediaWithCrops
{
    private readonly IApiMedia _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Models.DeliveryApi.ApiMediaWithCrops"/> class.
    /// </summary>
    /// <param name="inner">The media item to wrap.</param>
    /// <param name="focalPoint">The focal point of the image, or <c>null</c> if not specified.</param>
    /// <param name="crops">A collection of image crops, or <c>null</c> if not specified.</param>
    public ApiMediaWithCrops(
        IApiMedia inner,
        ImageFocalPoint? focalPoint,
        IEnumerable<ImageCrop>? crops)
    {
        _inner = inner;
        FocalPoint = focalPoint;
        Crops = crops;
    }

    /// <summary>
    /// Gets the unique identifier of the media.
    /// </summary>
    public Guid Id => _inner.Id;

    /// <summary>
    /// Gets the name of the media.
    /// </summary>
    public string Name => _inner.Name;

    /// <summary>
    /// Gets the media type of the inner media.
    /// </summary>
    public string MediaType => _inner.MediaType;

    /// <summary>
    /// Gets the URL used to access the media item.
    /// </summary>
    public string Url => _inner.Url;

    /// <summary>Gets the file extension of the media item.</summary>
    public string? Extension => _inner.Extension;

    /// <summary>
    /// Gets the width, in pixels, of the media item, or <c>null</c> if not available.
    /// </summary>
    public int? Width => _inner.Width;

    /// <summary>Gets the height of the media item.</summary>
    public int? Height => _inner.Height;

    /// <summary>
    /// Gets the size of the media in bytes.
    /// </summary>
    public int? Bytes => _inner.Bytes;

    /// <summary>
    /// Gets a dictionary containing the properties of the media item, where the key is the property alias and the value is the property value.
    /// </summary>
    public IDictionary<string, object?> Properties => _inner.Properties;

    /// <summary>
    /// Gets the focal point of the media item, if set.
    /// </summary>
    public ImageFocalPoint? FocalPoint { get; }

    /// <summary>
    /// Gets the collection of image crop definitions associated with this media item.
    /// </summary>
    public IEnumerable<ImageCrop>? Crops { get; }
}
