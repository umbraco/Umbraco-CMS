using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IScript" /> entities.
/// </summary>
public interface IScriptRepository : IReadRepository<string, IScript>, IWriteRepository<IScript>, IFileRepository,
    IFileWithFoldersRepository
{
}
