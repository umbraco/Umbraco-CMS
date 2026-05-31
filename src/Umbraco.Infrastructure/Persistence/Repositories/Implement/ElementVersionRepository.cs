using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents a repository for managing element content versions in the persistence layer.
/// </summary>
internal sealed class ElementVersionRepository : ContentVersionRepositoryBase<ElementDto, ElementVersionDto>, IElementVersionRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementVersionRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">An <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.</param>
    public ElementVersionRepository(IScopeAccessor scopeAccessor)
        : base(scopeAccessor)
    {
    }

    /// <inheritdoc />
    protected override string ContentDtoTableName => ElementDto.TableName;

    /// <inheritdoc />
    protected override string ContentVersionDtoTableName => ElementVersionDto.TableName;
}
