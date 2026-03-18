using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

/// <summary>
/// Provides services for managing and performing operations on stylesheet files and their hierarchical structure within the file system in Umbraco.
/// </summary>
public class StyleSheetTreeService : FileSystemTreeServiceBase, IStyleSheetTreeService
{
    private readonly IFileSystem _scriptFileSystem;

    protected override IFileSystem FileSystem => _scriptFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="StyleSheetTreeService"/> class.
    /// </summary>
    /// <param name="fileSystems">The <see cref="FileSystems"/> instance to be used for managing stylesheet files.</param>
    public StyleSheetTreeService(FileSystems fileSystems) =>
        _scriptFileSystem = fileSystems.StylesheetsFileSystem ??
                            throw new ArgumentException("Missing stylesheets file system", nameof(fileSystems));

    protected override bool FilterFile(string file) => file.ToLowerInvariant().EndsWith(".css");
}
