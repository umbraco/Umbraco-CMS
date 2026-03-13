namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

/// <summary>
/// Represents a request model used to create a new folder in the system.
/// </summary>
public class CreateFolderRequestModel : FolderModelBase
{
    /// <summary>
    /// Gets or sets the unique identifier of the folder.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Gets or sets a reference to the parent folder that will contain the new folder. Optional; if null, the folder is created at the root level.
    /// </summary>
    public ReferenceByIdModel? Parent { get; set; }
}
