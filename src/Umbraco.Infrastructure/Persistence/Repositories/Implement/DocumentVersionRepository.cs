using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class DocumentVersionRepository : ContentVersionRepositoryBase<DocumentDto, DocumentVersionDto>, IDocumentVersionRepository
{
    public DocumentVersionRepository(IScopeAccessor scopeAccessor)
        : base(scopeAccessor)
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">An <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.</param>
    {
    }

    protected override string ContentDtoTableName => DocumentDto.TableName;

    protected override string ContentVersionDtoTableName => DocumentVersionDto.TableName;
}
