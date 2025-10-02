using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public class StyleSheetTreeService : FileSystemTreeServiceBase, IStyleSheetTreeService
{
    private readonly IFileSystem _scriptFileSystem;

    protected override IFileSystem FileSystem => _scriptFileSystem;

    public StyleSheetTreeService(FileSystems fileSystems) =>
        _scriptFileSystem = fileSystems.StylesheetsFileSystem ??
                            throw new ArgumentException("Missing partial views file system", nameof(fileSystems));
}
