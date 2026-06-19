namespace Umbraco.Cms.Core.Models.DeliveryApi;

internal sealed class ApiMediaWithCropsResponse : ApiMediaWithCrops, IApiMediaWithCropsResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiMediaWithCropsResponse"/> class with the specified media item, focal point, crops, path, and dates.
    /// </summary>
    /// <param name="inner">The underlying media item represented by this response.</param>
    /// <param name="focalPoint">The optional focal point of the image, if defined.</param>
    /// <param name="crops">An optional collection of image crops associated with the media item.</param>
    /// <param name="path">The URL or path to the media item.</param>
    /// <param name="createDate">The date and time when the media item was created.</param>
    /// <param name="updateDate">The date and time when the media item was last updated.</param>
    /// <param name="altText">The alternative text for the media item, or <c>null</c> if not specified.</param>
    public ApiMediaWithCropsResponse(
        IApiMedia inner,
        ImageFocalPoint? focalPoint,
        IEnumerable<ImageCrop>? crops,
        string path,
        DateTime createDate,
        DateTime updateDate,
        string? altText = null)
        : base(inner, focalPoint, crops, altText)
    {
        Path = path;
        CreateDate = createDate;
        UpdateDate = updateDate;
    }

    /// <summary>
    /// Gets the URL path of the media item.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the date and time when the media item was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    /// Gets the date and time when the media was last updated.
    /// </summary>
    public DateTime UpdateDate { get; }
}
