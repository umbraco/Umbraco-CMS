namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// Represents the API request model used for creating and publishing a new element in Umbraco.
/// </summary>
public class CreateAndPublishElementRequestModel : CreateElementRequestModel
{
    /// <summary>
    /// The cultures to publish after creating the element.
    /// </summary>
    public string[] CulturesToPublish { get; set; } = [];
}
