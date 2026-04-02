using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="PublicAccessEntry" /> entities.
/// </summary>
public interface IPublicAccessRepository : IReadWriteQueryRepository<Guid, PublicAccessEntry>
{
}
