namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents a request model used for updating and publishing an element via the API.
/// </summary>
public class UpdateAndPublishElementRequestModel : UpdateElementRequestModel
{
    /// <summary>
    /// The cultures to publish after updating the element.
    /// </summary>
    public string[] CulturesToPublish { get; set; } = [];
}
