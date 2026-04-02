using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IRelationType" /> entities.
/// </summary>
public interface IRelationTypeRepository : IReadWriteQueryRepository<int, IRelationType>,
    IReadRepository<Guid, IRelationType>
{
}
