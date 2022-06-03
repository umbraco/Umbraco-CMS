using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IPartialViewRepository : IAsyncReadRepository<string, IPartialView>, IAsyncWriteRepository<IPartialView>, IFileRepository, IFileWithFoldersRepository
    {
    }
}
