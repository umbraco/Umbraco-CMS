using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class DocumentVersionRepository : ContentVersionRepositoryBase<DocumentDto, DocumentVersionDto>, IDocumentVersionRepository
{
    public DocumentVersionRepository(IScopeAccessor scopeAccessor)
        : base(scopeAccessor)
    {
    }

    protected override string ContentDtoTableName => DocumentDto.TableName;

    protected override string ContentVersionDtoTableName => DocumentVersionDto.TableName;
}
