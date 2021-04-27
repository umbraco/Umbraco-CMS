using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IScriptRepository : IReadRepository<string, IScript>, IWriteRepository<IScript>
    {
        void AddFolder(string folderPath);
        void DeleteFolder(string folderPath);
    }
}
