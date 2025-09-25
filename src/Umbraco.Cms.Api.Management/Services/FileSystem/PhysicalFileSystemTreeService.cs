using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Api.Management.Services.FileSystem;

public class PhysicalFileSystemTreeService : FileSystemTreeServiceBase, IPhysicalFileSystemTreeService
{
    private readonly IFileSystem _physicalFileSystem;

    protected override IFileSystem FileSystem => _physicalFileSystem;

    public PhysicalFileSystemTreeService(IPhysicalFileSystem physicalFileSystem) =>
        _physicalFileSystem = physicalFileSystem;
}
