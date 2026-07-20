namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents content in the Delivery API.
/// </summary>
public class ApiContent : ApiElement, IApiContent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContent" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the content.</param>
    /// <param name="name">The name of the content.</param>
    /// <param name="contentType">The content type alias.</param>
    /// <param name="createDate">The date and time when the content was created.</param>
    /// <param name="updateDate">The date and time when the content was last updated.</param>
    /// <param name="route">The route information for the content.</param>
    /// <param name="properties">The property values of the content.</param>
    public ApiContent(Guid id, string name, string contentType, DateTime createDate, DateTime updateDate, IApiContentRoute route, IDictionary<string, object?> properties)
        : base(id, contentType, properties)
    {
        Name = name;
        CreateDate = createDate;
        UpdateDate = updateDate;
        Route = route;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public DateTime CreateDate { get; }

    /// <inheritdoc />
    public DateTime UpdateDate { get; }

    /// <inheritdoc />
    public IApiContentRoute Route { get; }
}
