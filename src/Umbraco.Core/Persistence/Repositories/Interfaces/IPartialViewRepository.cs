using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IPartialViewRepository : IRepository<string, IPartialView>
    {
        void AddFolder(string folderPath);
        void DeleteFolder(string folderPath);
    }
}