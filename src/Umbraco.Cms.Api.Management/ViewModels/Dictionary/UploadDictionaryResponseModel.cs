namespace Umbraco.Cms.Api.Management.ViewModels.Dictionary;

/// <summary>
/// Represents the response returned after uploading a dictionary in the management API.
/// </summary>
public class UploadDictionaryResponseModel
{
    /// <summary>
    /// Gets or sets the collection of dictionary items that were imported as part of the upload operation.
    /// </summary>
    public IEnumerable<ImportDictionaryItemsPresentationModel> DictionaryItems { get; set; } = Enumerable.Empty<ImportDictionaryItemsPresentationModel>();

    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public string? FileName { get; set; }
}
