using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Provides functionality for managing and interacting with the partial view file system tree in Umbraco.
/// </summary>
public class PartialViewTreeService : FileSystemTreeServiceBase, IPartialViewTreeService
{
    private readonly IFileSystem _partialViewFileSystem;

    protected override IFileSystem FileSystem => _partialViewFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewTreeService"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance used to manage partial view files within the service.</param>
    public PartialViewTreeService(FileSystems fileSystems) =>
        _partialViewFileSystem = fileSystems.PartialViewsFileSystem ??
                                 throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    protected override bool FilterFile(string file) => file.ToLowerInvariant().EndsWith(".cshtml");
}
