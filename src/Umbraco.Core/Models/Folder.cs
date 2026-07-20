using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a folder entity in the file system.
/// </summary>
public sealed class Folder : EntityBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Folder" /> class.
    /// </summary>
    /// <param name="folderPath">The path of the folder.</param>
    public Folder(string folderPath) => Path = folderPath;

    /// <summary>
    ///     Gets or sets the path of the folder.
    /// </summary>
    public string Path { get; set; }
}
