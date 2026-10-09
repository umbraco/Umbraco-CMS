namespace Umbraco.Cms.Api.Management.ViewModels.DynamicRoot;

/// <summary>
/// Represents the response model containing information about a dynamic root entity in the Umbraco CMS Management API.
/// </summary>
public class DynamicRootResponseModel
{
    /// <summary>
    /// Gets or sets the collection of dynamic root identifiers. The content root is never included; a query that resolves to it returns an empty collection.
    /// </summary>
    public IEnumerable<Guid> Roots { get; set; } = Enumerable.Empty<Guid>();
}
