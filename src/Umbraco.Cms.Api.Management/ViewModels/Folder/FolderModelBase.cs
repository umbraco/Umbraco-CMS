using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Folder;

/// <summary>
/// Provides a base view model for folder representations in the Umbraco CMS Management API.
/// </summary>
public class FolderModelBase
{
    /// <summary>
    /// Gets or sets the name of the folder.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;
}
