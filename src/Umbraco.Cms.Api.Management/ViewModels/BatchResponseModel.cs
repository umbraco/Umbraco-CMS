namespace Umbraco.Cms.Api.Management.ViewModels;

/// <summary>
/// Represents a response model for retrieving multiple entities, including the total count and the collection of items.
/// </summary>
/// <typeparam name="T">The entity response model type.</typeparam>
public class BatchResponseModel<T>
{
    /// <summary>
    /// Gets or sets the total number of entities returned.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the collection of entities.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = [];
}
