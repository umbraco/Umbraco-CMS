using MessagePack;
using MessagePack.Resolvers;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Persistence;

public class IndexDocumentRepository : IIndexDocumentRepository
{
    private readonly IScopeAccessor _scopeAccessor;
    private readonly MessagePackSerializerOptions _options;

    public IndexDocumentRepository(IScopeAccessor scopeAccessor)
    {
        _scopeAccessor = scopeAccessor;

        MessagePackSerializerOptions defaultOptions = ContractlessStandardResolver.Options;
        IFormatterResolver resolver = CompositeResolver.Create(defaultOptions.Resolver);
        _options = defaultOptions
            .WithResolver(resolver)
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithSecurity(MessagePackSecurity.UntrustedData);
    }

    public async Task AddAsync(IndexDocument indexDocument)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("Cannot add document as there is no ambient scope.");
        }

        IndexDocumentDto dto = ToDto(indexDocument);
        await _scopeAccessor.AmbientScope.Database.InsertAsync(dto);
    }

    public async Task<IndexDocument?> GetAsync(Guid id, bool published)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("Cannot get document as there is no ambient scope.");
        }

        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Select<IndexDocumentDto>()
            .From<IndexDocumentDto>()
            .Where<IndexDocumentDto>(x => x.Key == id && x.Published == published);

        IndexDocumentDto? documentDto = await _scopeAccessor.AmbientScope.Database.FirstOrDefaultAsync<IndexDocumentDto>(sql);

        return ToDocument(documentDto);
    }

    public async Task DeleteAsync(Guid[] ids, bool published)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("Cannot delete document as there is no ambient scope.");
        }

        List<Guid> idsAsList = [..ids];
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Delete<IndexDocumentDto>()
            .Where<IndexDocumentDto>(x => idsAsList.Contains(x.Key) && x.Published == published);

        await _scopeAccessor.AmbientScope.Database.ExecuteAsync(sql);
    }

    public async Task DeleteAllAsync()
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("Cannot delete all documents as there is no ambient scope.");
        }

        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Delete<IndexDocumentDto>();

        await _scopeAccessor.AmbientScope.Database.ExecuteAsync(sql);
    }

    public async Task<PagedModel<IndexDocument>> GetPagedAsync(long currentPage, int pageSize)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("Cannot fetch paged documents as there is no ambient scope.");
        }

        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql().SelectAll().From<IndexDocumentDto>();
        Page<IndexDocumentDto> page =
            await _scopeAccessor.AmbientScope.Database.PageAsync<IndexDocumentDto>(currentPage, pageSize, sql);

        return new PagedModel<IndexDocument>
        {
            Total = page.TotalItems,
            Items = page.Items.Select(ToDocument).WhereNotNull().ToArray(),
        };
    }

    private IndexDocumentDto ToDto(IndexDocument indexDocument) =>
        new()
        {
            Key = indexDocument.Key,
            Published = indexDocument.Published,
            Fields = MessagePackSerializer.Serialize(indexDocument.Fields, _options),
        };

    private IndexDocument? ToDocument(IndexDocumentDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return new IndexDocument
        {
            Key = dto.Key,
            Fields = MessagePackSerializer.Deserialize<IndexField[]>(dto.Fields, _options) ?? [],
            Published = dto.Published,
        };
    }
}
