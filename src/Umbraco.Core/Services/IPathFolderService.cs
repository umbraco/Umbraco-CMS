using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPathFolderService
{
    Task<PathContainer?> GetAsync(string path);
}
