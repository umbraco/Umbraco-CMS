using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IStylesheet" /> entities.
/// </summary>
public interface IStylesheetRepository : IReadRepository<string, IStylesheet>, IWriteRepository<IStylesheet>,
    IFileRepository, IFileWithFoldersRepository
{
}
