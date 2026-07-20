namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents the response model returned by the Management API for a dictionary item, typically used for localization and multilingual content in Umbraco.
/// </summary>
public class DictionaryItemResponseModel : DictionaryItemModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the dictionary item.
    /// </summary>
    public Guid Id { get; set; }
}
