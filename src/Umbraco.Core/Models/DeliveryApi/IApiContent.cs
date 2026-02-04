using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents content in the Delivery API.
/// </summary>
[JsonDerivedType(typeof(ApiContent))]
public interface IApiContent : IApiElement
{
    /// <summary>
    ///     Gets the name of the content.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets the date and time when the content was created.
    /// </summary>
    public DateTime CreateDate { get; }

    /// <summary>
    ///     Gets the date and time when the content was last updated.
    /// </summary>
    public DateTime UpdateDate { get; }

    /// <summary>
    ///     Gets the route information for the content.
    /// </summary>
    IApiContentRoute Route { get; }
}
