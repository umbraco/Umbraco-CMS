namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents the data required to create a new dictionary item via the API.
/// </summary>
public class CreateDictionaryItemRequestModel : DictionaryItemModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the dictionary item.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent dictionary item, if any.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
