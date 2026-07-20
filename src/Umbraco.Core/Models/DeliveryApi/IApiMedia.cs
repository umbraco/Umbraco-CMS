namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents media in the Delivery API.
/// </summary>
public interface IApiMedia
{
    /// <summary>
    ///     Gets the unique identifier of the media.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Gets the name of the media.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the media type alias.
    /// </summary>
    string MediaType { get; }

    /// <summary>
    ///     Gets the URL of the media.
    /// </summary>
    string Url { get; }

    /// <summary>
    ///     Gets the file extension of the media.
    /// </summary>
    string? Extension { get; }

    /// <summary>
    ///     Gets the width of the media in pixels, if applicable.
    /// </summary>
    int? Width { get; }

    /// <summary>
    ///     Gets the height of the media in pixels, if applicable.
    /// </summary>
    int? Height { get; }

    /// <summary>
    ///     Gets the size of the media in bytes.
    /// </summary>
    int? Bytes { get; }

    /// <summary>
    ///     Gets the property values of the media.
    /// </summary>
    IDictionary<string, object?> Properties { get; }
}
