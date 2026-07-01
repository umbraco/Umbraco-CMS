namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents the data required to move a dictionary item within the system.
/// </summary>
public class MoveDictionaryRequestModel
{
    /// <summary>
    /// Gets or sets the target parent dictionary item by ID to which the dictionary item will be moved.
    /// </summary>
    public ReferenceByIdModel? Target { get; set; }
}
