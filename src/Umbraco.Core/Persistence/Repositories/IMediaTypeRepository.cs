using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMediaType" /> entities.
/// </summary>
/// <remarks>
///     Implements both the synchronous <see cref="IContentTypeRepositoryBase{TItem}"/> (still used by the
///     synchronous media service call sites) and the asynchronous
///     <see cref="IAsyncContentTypeRepositoryBase{TItem}"/> (required by <see cref="AsyncContentTypeServiceBase{TRepository,TItem}"/>).
/// </remarks>
// TODO (EFCore): Drop IContentTypeRepositoryBase<IMediaType> here once MediaTypeService has moved to
// AsyncContentTypeServiceBase, so this can go async-only like IContentTypeRepository.
public interface IMediaTypeRepository : IContentTypeRepositoryBase<IMediaType>, IAsyncContentTypeRepositoryBase<IMediaType>
{
}
