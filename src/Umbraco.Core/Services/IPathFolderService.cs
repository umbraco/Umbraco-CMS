using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPathFolderService<TStatus> where TStatus : Enum
{
    Task<PathContainer?> GetAsync(string path);

    Task<Attempt<PathContainer?, TStatus>> CreateAsync(PathContainer container);

    Task<Attempt<TStatus?>> DeleteAsync(string path);
}
