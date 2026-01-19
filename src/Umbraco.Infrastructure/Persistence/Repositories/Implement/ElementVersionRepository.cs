using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class ElementVersionRepository : ContentVersionRepositoryBase<ElementDto, ElementVersionDto>, IElementVersionRepository
{
    public ElementVersionRepository(IScopeAccessor scopeAccessor)
        : base(scopeAccessor)
    {
    }

    protected override string ContentDtoTableName => ElementDto.TableName;

    protected override string ContentVersionDtoTableName => ElementVersionDto.TableName;
}
