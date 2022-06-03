using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IStylesheetRepository : IAsyncReadRepository<string, IStylesheet>, IAsyncWriteRepository<IStylesheet>, IFileRepository, IFileWithFoldersRepository
    {
    }
}
