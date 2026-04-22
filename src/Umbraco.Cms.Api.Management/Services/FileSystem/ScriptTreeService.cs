using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Provides functionality for managing and interacting with the script files within the file system tree.
/// </summary>
public class ScriptTreeService : FileSystemTreeServiceBase, IScriptTreeService
{
    private readonly IFileSystem _scriptFileSystem;

    protected override IFileSystem FileSystem => _scriptFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Services.FileSystem.ScriptTreeService"/> class, which provides functionality for managing and retrieving script files from the file system.
    /// </summary>
    /// <param name="fileSystems">An instance of <see cref="FileSystems"/> used to access and manage file system providers for scripts.</param>
    public ScriptTreeService(FileSystems fileSystems) =>
        _scriptFileSystem = fileSystems.ScriptsFileSystem ??
                            throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    protected override bool FilterFile(string file) => file.ToLowerInvariant().EndsWith(".js");
}
