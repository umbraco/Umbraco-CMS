using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

public abstract class PathFolderServiceBase<TRepo> : IPathFolderService
    where TRepo: IFileWithFoldersRepository
{
    public abstract TRepo Repository { get; }

    public virtual Task<PathContainer?> GetAsync(string path)
    {
        // There's not much we can actually get when it's a folder, so it more a matter of ensuring the folder exists and returning a model.
        if (Repository.FolderExists(path) is false)
        {
            return Task.FromResult<PathContainer?>(null);
        }

        var parentPath = Path.GetDirectoryName(path);
        var parentPathLength = parentPath?.Length + 1 ?? 0;

        var model = new PathContainer
        {
            Name = path.Remove(0, parentPathLength),
            ParentPath = parentPath,
        };

        return Task.FromResult<PathContainer?>(model);
    }
}
