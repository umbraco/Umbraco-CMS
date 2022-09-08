using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPartialViewRepository : IReadRepository<string, IPartialView>, IWriteRepository<IPartialView>,
    IFileRepository, IFileWithFoldersRepository
{
}
