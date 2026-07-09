using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Provides functionality for managing and navigating the physical file system as a tree structure.
/// </summary>
public class PhysicalFileSystemTreeService : FileSystemTreeServiceBase, IPhysicalFileSystemTreeService
{
    private static readonly string[] _allowedRootFolders = { $"{Path.DirectorySeparatorChar}App_Plugins", $"{Path.DirectorySeparatorChar}wwwroot" };

    private readonly IFileSystem _physicalFileSystem;

    protected override IFileSystem FileSystem => _physicalFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalFileSystemTreeService"/> class with the specified physical file system.
    /// </summary>
    /// <param name="physicalFileSystem">The <see cref="IPhysicalFileSystem"/> instance to be used by the service.</param>
    public PhysicalFileSystemTreeService(IPhysicalFileSystem physicalFileSystem) =>
        _physicalFileSystem = physicalFileSystem;

    /// <inheritdoc/>
    public override string[] GetDirectories(string path) =>
        IsTreeRootPath(path)
            ? _allowedRootFolders
            : IsAllowedPath(path)
                ? base.GetDirectories(path)
                : Array.Empty<string>();

    /// <inheritdoc/>
    public override string[] GetFiles(string path)
        => IsTreeRootPath(path) || IsAllowedPath(path) is false
            ? []
            : base.GetFiles(path);

    private static bool IsTreeRootPath(string path) => path == Path.DirectorySeparatorChar.ToString();

    private static bool IsAllowedPath(string path) => _allowedRootFolders.Contains(path) || _allowedRootFolders.Any(folder => path.StartsWith($"{folder}{Path.DirectorySeparatorChar}"));

}
