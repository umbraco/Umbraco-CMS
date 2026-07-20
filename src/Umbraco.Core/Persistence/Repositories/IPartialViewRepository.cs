using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IPartialView" /> entities.
/// </summary>
public interface IPartialViewRepository : IReadRepository<string, IPartialView>, IWriteRepository<IPartialView>,
    IFileRepository, IFileWithFoldersRepository
{
}
