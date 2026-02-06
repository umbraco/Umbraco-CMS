namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a media response with cropping information in the Delivery API.
/// </summary>
public interface IApiMediaWithCropsResponse : IApiMediaWithCrops
{
    /// <summary>
    ///     Gets the path of the media file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    ///     Gets the date and time when the media was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the date and time when the media was last updated.
    /// </summary>
    public DateTime UpdateDate { get; }
}
