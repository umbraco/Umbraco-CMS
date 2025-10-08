using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public class PartialViewTreeService : FileSystemTreeServiceBase, IPartialViewTreeService
{
    private readonly IFileSystem _partialViewFileSystem;

    protected override IFileSystem FileSystem => _partialViewFileSystem;

    public PartialViewTreeService(FileSystems fileSystems) =>
        _partialViewFileSystem = fileSystems.PartialViewsFileSystem ??
                                 throw new ArgumentException("Missing partial views file system", nameof(fileSystems));
}
