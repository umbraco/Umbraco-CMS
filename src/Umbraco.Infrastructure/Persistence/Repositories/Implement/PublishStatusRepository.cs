using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class PublishStatusRepository: IPublishStatusRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public PublishStatusRepository(IScopeAccessor scopeAccessor)
        => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    private Sql<ISqlContext>  GetBaseQuery()
    {
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select(
                $"n.{NodeDto.KeyColumnName}",
                $"l.{LanguageDto.IsoCodeColumnName}",
                $"ct.{ContentTypeDto.VariationsColumnName}",
                $"d.{DocumentDto.PublishedColumnName}",
                $"COALESCE(dcv.{DocumentCultureVariationDto.PublishedColumnName}, 0) as {PublishStatusDto.DocumentVariantPublishStatusColumnName}")
            .From<DocumentDto>("d")
            .InnerJoin<ContentDto>("c").On<DocumentDto, ContentDto>((d, c) => d.NodeId == c.NodeId, "c", "d")
            .InnerJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .CrossJoin<LanguageDto>("l")
            .LeftJoin<DocumentCultureVariationDto>("dcv").On<LanguageDto, DocumentCultureVariationDto, DocumentDto >((l, dcv, d) => l.Id == dcv.LanguageId && d.NodeId == dcv.NodeId , "l", "dcv", "d")
            .InnerJoin<NodeDto>("n").On<DocumentDto, NodeDto>((d, n) => n.NodeId == d.NodeId, "d", "n")
            ;

        return sql;
    }


    public async Task<IDictionary<Guid, ISet<string>>> GetAllPublishStatusAsync(CancellationToken cancellationToken)
    {
        Sql<ISqlContext> sql = GetBaseQuery();

        List<PublishStatusDto>? databaseRecords = await Database.FetchAsync<PublishStatusDto>(sql);

        return Map(databaseRecords);
    }

    public async Task<ISet<string>> GetPublishStatusAsync(Guid documentKey, CancellationToken cancellationToken)
    {
        Sql<ISqlContext> sql = GetBaseQuery();
        sql = sql.Where<NodeDto>(n => n.UniqueId == documentKey, "n");

        List<PublishStatusDto>? databaseRecords = await Database.FetchAsync<PublishStatusDto>(sql);

        IDictionary<Guid, ISet<string>> result = Map(databaseRecords);
        return result.TryGetValue(documentKey, out ISet<string>? value) ? value : new HashSet<string>();
    }

    public async Task<IDictionary<Guid, ISet<string>>> GetDescendantsOrSelfPublishStatusAsync(Guid rootDocumentKey, CancellationToken cancellationToken)
    {
        var pathSql = Database.SqlContext.Sql()
            .Select<NodeDto>(x => x.Path)
            .From<NodeDto>()
            .Where<NodeDto>(x => x.UniqueId == rootDocumentKey);
        var rootPath = await Database.ExecuteScalarAsync<string>(pathSql);

        Sql<ISqlContext> sql = GetBaseQuery()
            .InnerJoin<NodeDto>("rn").On<NodeDto, NodeDto>((n, rn) => n.Path.StartsWith(rootPath), "n", "rn") //rn = root node
            .Where<NodeDto>(rn => rn.UniqueId == rootDocumentKey, "rn");

        List<PublishStatusDto>? databaseRecords = await Database.FetchAsync<PublishStatusDto>(sql);

        IDictionary<Guid, ISet<string>> result = Map(databaseRecords);

        return result;
    }

    private IDictionary<Guid, ISet<string>> Map(List<PublishStatusDto> databaseRecords)
    {
        return databaseRecords
            .GroupBy(x => x.Key)
            .ToDictionary(
                x=>x.Key,
                x=> (ISet<string>) x.Where(x=> IsPublished(x)).Select(y=>y.IsoCode).ToHashSet());
    }

    private static bool IsPublished(PublishStatusDto publishStatusDto)
    {
        switch ((ContentVariation)publishStatusDto.ContentTypeVariation)
        {
            case ContentVariation.Culture:
            case ContentVariation.CultureAndSegment:
                return publishStatusDto.DocumentVariantPublishStatus;
            case ContentVariation.Nothing:
            case ContentVariation.Segment:
            default:
                return publishStatusDto.DocumentInvariantPublished;
        }
    }

    private sealed class PublishStatusDto
    {

        public const string DocumentVariantPublishStatusColumnName = "variantPublished";


        [Column(NodeDto.KeyColumnName)]
        public Guid Key { get; set; }

        [Column(LanguageDto.IsoCodeColumnName)]
        public string IsoCode { get; set; } = string.Empty;

        [Column(ContentTypeDto.VariationsColumnName)]
        public byte ContentTypeVariation  { get; set; }

        [Column(DocumentDto.PublishedColumnName)]
        public bool DocumentInvariantPublished  { get; set; }

        [Column(DocumentVariantPublishStatusColumnName)]
        public bool DocumentVariantPublishStatus  { get; set; }
    }
}
