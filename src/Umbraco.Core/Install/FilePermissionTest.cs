namespace Umbraco.Cms.Core.Install;

/// <summary>
///     Defines the types of file permission tests performed during Umbraco installation.
/// </summary>
public enum FilePermissionTest
{
    /// <summary>
    ///     Test for the ability to create folders.
    /// </summary>
    FolderCreation,

    /// <summary>
    ///     Test for the ability to write files required for packages.
    /// </summary>
    FileWritingForPackages,

    /// <summary>
    ///     Test for general file writing ability.
    /// </summary>
    FileWriting,

    /// <summary>
    ///     Test for the ability to create folders in the media directory.
    /// </summary>
    MediaFolderCreation,
}
