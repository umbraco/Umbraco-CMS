using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Repository responsible for managing and retrieving publish status information for content items.
/// </summary>
public class PublishStatusRepository : IPublishStatusRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishStatusRepository"/> class with the specified scope accessor.
    /// </summary>
    /// <param name="scopeAccessor">An accessor for managing the database scope within which repository operations are performed.</param>
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

    private Sql<ISqlContext> GetBaseQuery()
    {
        SqlSyntax.ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        Sql<ISqlContext> sql = Database.SqlContext.Sql()
            .Select(
                $"n.{syntax.GetQuotedColumnName(NodeDto.KeyColumnName)}",
                $"l.{syntax.GetQuotedColumnName(LanguageDto.IsoCodeColumnName)}",
                $"ct.{syntax.GetQuotedColumnName(ContentTypeDto.VariationsColumnName)}",
                $"d.{syntax.GetQuotedColumnName(DocumentDto.PublishedColumnName)}",
                $"dcv.{syntax.GetQuotedColumnName(DocumentCultureVariationDto.PublishedColumnName)} as {syntax.GetQuotedColumnName(PublishStatusDto.DocumentVariantPublishStatusColumnName)}") // COALESCE is not necessary as the column is not nullable
            .From<DocumentDto>("d")
            .InnerJoin<ContentDto>("c").On<DocumentDto, ContentDto>((d, c) => d.NodeId == c.NodeId, "c", "d")
            .InnerJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .CrossJoin<LanguageDto>("l")
            .LeftJoin<DocumentCultureVariationDto>("dcv").On<LanguageDto, DocumentCultureVariationDto, DocumentDto>((l, dcv, d) => l.Id == dcv.LanguageId && d.NodeId == dcv.NodeId, "l", "dcv", "d")
            .InnerJoin<NodeDto>("n").On<DocumentDto, NodeDto>((d, n) => n.NodeId == d.NodeId, "d", "n");

        return sql;
    }


    /// <summary>
    /// Asynchronously retrieves all publish statuses for content items.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a dictionary where each key is the GUID of a content item, and each value is a set of publish status strings associated with that item.
    /// </returns>
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

    /// <summary>
    /// Asynchronously retrieves the publish status for the specified root document and all its descendants.
    /// </summary>
    /// <param name="rootDocumentKey">The unique identifier (GUID) of the root document whose publish status and that of its descendants will be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The result is a dictionary mapping each document's unique identifier to a set of publish status values (as strings) for that document.
    /// </returns>
    public async Task<IDictionary<Guid, ISet<string>>> GetDescendantsOrSelfPublishStatusAsync(Guid rootDocumentKey, CancellationToken cancellationToken)
    {
        var pathSql = Database.SqlContext.Sql()
            .Select<NodeDto>(x => x.Path)
            .From<NodeDto>()
            .Where<NodeDto>(x => x.UniqueId == rootDocumentKey);
        var rootPath = await Database.ExecuteScalarAsync<string>(pathSql);
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            return new Dictionary<Guid, ISet<string>>();
        }

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
                x => x.Key,
                x => (ISet<string>)x.Where(x => IsPublished(x)).Select(y => y.IsoCode).ToHashSet());
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


        /// <summary>
        /// Gets or sets the unique key that identifies the publish status.
        /// </summary>
        [Column(NodeDto.KeyColumnName)]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the ISO code representing the language.
        /// </summary>
        [Column(LanguageDto.IsoCodeColumnName)]
        public string IsoCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content type variation, stored as a byte value representing the variation flags or mode for the content type.
        /// </summary>
        [Column(ContentTypeDto.VariationsColumnName)]
        public byte ContentTypeVariation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document is published for all cultures (invariantly).
        /// </summary>
        [Column(DocumentDto.PublishedColumnName)]
        public bool DocumentInvariantPublished { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the document variant is published.
        /// </summary>
        [Column(DocumentVariantPublishStatusColumnName)]
        public bool DocumentVariantPublishStatus { get; set; }
    }
}
