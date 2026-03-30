namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// A view model used for importing dictionary items in the Umbraco CMS management API.
/// </summary>
public class ImportDictionaryItemsPresentationModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the dictionary item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the dictionary item.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the model representing the parent dictionary item by its ID.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
