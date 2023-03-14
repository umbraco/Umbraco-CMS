using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IStylesheetRepository : IReadRepository<string, IStylesheet>, IWriteRepository<IStylesheet>,
    IFileRepository, IFileWithFoldersRepository
{
}
