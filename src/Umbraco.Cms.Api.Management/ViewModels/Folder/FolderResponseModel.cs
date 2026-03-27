namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

/// <summary>
/// Represents a model used to return folder information in API responses.
/// </summary>
public class FolderResponseModel : FolderModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the folder.
    /// </summary>
    public Guid Id { get; set; }
}
