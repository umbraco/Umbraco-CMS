using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMediaType" /> entities.
/// </summary>
// TODO (EFCore): Once this repository migrates to EF Core, add IAsyncContentTypeRepositoryBase<IMediaType> here
// (see IMemberTypeRepository) — and then IMemberTypeRepository can drop IContentTypeRepositoryBase<IMemberType>
// entirely, going async-only like IContentTypeRepository.
public interface IMediaTypeRepository : IContentTypeRepositoryBase<IMediaType>
{
}
