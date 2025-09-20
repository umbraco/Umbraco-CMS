using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public class ScriptTreeService : FileSystemTreeServiceBase, IScriptTreeService
{
    private readonly IFileSystem _scriptFileSystem;

    protected override IFileSystem FileSystem => _scriptFileSystem;

    public ScriptTreeService(FileSystems fileSystems) =>
        _scriptFileSystem = fileSystems.ScriptsFileSystem ??
                            throw new ArgumentException("Missing partial views file system", nameof(fileSystems));
}
