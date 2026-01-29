namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Represents a request model that specifies a collection of entity identifiers to be fetched.
/// </summary>
public class FetchRequestModel
{
    /// <summary>
    /// Gets or sets the entity identifiers to be fetched.
    /// </summary>
    public ReferenceByIdModel[] Ids { get; set; } = [];
}
