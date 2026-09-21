using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMemberType" /> entities.
/// </summary>
/// <remarks>
///     Implements both the synchronous <see cref="IContentTypeRepositoryBase{TItem}"/> (still used by the
///     synchronous member repository/service call sites) and the asynchronous
///     <see cref="IAsyncContentTypeRepositoryBase{TItem}"/> (required by <see cref="AsyncContentTypeServiceBase{TRepository,TItem}"/>,
///     used by <see cref="Umbraco.Cms.Core.Services.MemberTypeService"/>).
/// </remarks>
// TODO (EFCore): Drop IContentTypeRepositoryBase<IMemberType> here once MemberTypeService no longer needs it via
// the shared sync base, so this can go async-only like IContentTypeRepository.
public interface IMemberTypeRepository : IContentTypeRepositoryBase<IMemberType>, IAsyncContentTypeRepositoryBase<IMemberType>
{
}
