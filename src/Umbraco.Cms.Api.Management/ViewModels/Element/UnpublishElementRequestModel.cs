namespace Umbraco.Cms.Api.Management.ViewModels.Element;

/// <summary>
/// A request model used to unpublish an element.
/// </summary>
public class UnpublishElementRequestModel
{
    /// <summary>
    /// Gets or sets the set of cultures for which the element should be unpublished.
    /// </summary>
    public ISet<string>? Cultures { get; set; }
}
