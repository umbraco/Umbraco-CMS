using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_10_0;

/// <summary>
/// Requests a rebuild of the database cache when a published document that can be restored with its published state
/// has no published cache row.
/// </summary>
/// <remarks>
/// Versions 17.4.0 to 17.9.x deleted the cache rows of content moved to the recycle bin. Restoring that content does
/// not rewrite the published rows of its descendants, so they stay out of the published cache after their ancestor is
/// published again. Published descendants still in the recycle bin are therefore included, while items at the top of
/// the recycle bin are ignored (restoring them unpublishes them) and documents without a published version are ignored
/// (the rebuild would not write a published row for them either). Sites with no affected documents are not rebuilt.
/// </remarks>
public class RebuildCacheForMissingPublishedRows : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildCacheForMissingPublishedRows"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public RebuildCacheForMissingPublishedRows(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    protected override async Task MigrateAsync()
    {
        Sql<ISqlContext> sql = Sql()
            .Select<DocumentDto>(document => document.NodeId)
            .From<DocumentDto>()
            .InnerJoin<NodeDto>()
            .On<DocumentDto, NodeDto>((document, node) => document.NodeId == node.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<DocumentDto, ContentVersionDto>((document, version) => document.NodeId == version.NodeId)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>((version, documentVersion) => version.Id == documentVersion.Id && documentVersion.Published)
            .LeftJoin<ContentNuDto>()
            .On<DocumentDto, ContentNuDto>((document, cacheRow) => document.NodeId == cacheRow.NodeId && cacheRow.Published == true)
            .Where<DocumentDto>(document => document.Published)
            .Where<NodeDto>(node => node.ParentId != Constants.System.RecycleBinContent)
            .WhereNull<ContentNuDto>(cacheRow => cacheRow.NodeId)
            .SelectTop(1);

        List<int> affectedDocuments = await Database.FetchAsync<int>(sql);

        RebuildCache = affectedDocuments.Count > 0;
    }
}
