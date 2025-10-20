using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public class StyleSheetTreeService : FileSystemTreeServiceBase, IStyleSheetTreeService
{
    private readonly IFileSystem _scriptFileSystem;

    protected override IFileSystem FileSystem => _scriptFileSystem;

    public StyleSheetTreeService(FileSystems fileSystems) =>
        _scriptFileSystem = fileSystems.StylesheetsFileSystem ??
                            throw new ArgumentException("Missing stylesheets file system", nameof(fileSystems));

    public override string[] GetFiles(string path) => FileSystem
        .GetFiles(path)
        .Where(file => file.EndsWith(".css"))
        .OrderBy(file => file)
        .ToArray();
}
