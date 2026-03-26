using System;
using System.Data;
using System.Text;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class DocumentVersionRepository : IDocumentVersionRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentVersionRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">An <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.</param>
    public DocumentVersionRepository(IScopeAccessor scopeAccessor) =>
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup()
#pragma warning restore CS0618 // Type or member is obsolete
        => GetDocumentVersionsEligibleForCleanup(olderThan: null, maxCount: null);

    /// <inheritdoc />
    public IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup(DateTime olderThan, int? maxCount)

        // TODO (V19): When the obsolete overload is removed, bring the helper method into here and use non-nullable parameters.
        => GetDocumentVersionsEligibleForCleanup((DateTime?)olderThan, maxCount);

    private IReadOnlyCollection<ContentVersionMeta> GetDocumentVersionsEligibleForCleanup(DateTime? olderThan, int? maxCount)
    {
        IScope? ambientScope = _scopeAccessor.AmbientScope;
        if (ambientScope is null)
        {
            return [];
        }

        ISqlSyntaxProvider syntax = ambientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> query = ambientScope.SqlContext.Sql()
            .Select(GetQuotedSelectColumns(syntax))
            .From<DocumentDto>()
            .InnerJoin<ContentDto>()
            .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
            .LeftJoin<UserDto>()
            .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
            .Where<ContentVersionDto>(x => !x.Current) // Never delete current draft version
            .Where<ContentVersionDto>(x => !x.PreventCleanup) // Never delete "pinned" versions
            .Where<DocumentVersionDto>(x => !x.Published); // Never delete published version

        if (olderThan.HasValue)
        {
            query = query.Where<ContentVersionDto>(x => x.VersionDate < olderThan.Value);
        }

        query = query
            .OrderBy<ContentVersionDto>(x => x.VersionDate)
            .OrderBy<ContentVersionDto>(x => x.Id);

        if (maxCount.HasValue)
        {
            query = query.SelectTop(maxCount.Value);
        }

        List<ContentVersionMeta> results = ambientScope.Database.Fetch<ContentVersionMeta>(query);
        EnsureUtcDates(results);
        return results;
    }

    private string GetQuotedSelectColumns(ISqlSyntaxProvider syntax) =>
        $@"
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "id", "versionId")},
{syntax.ColumnWithAlias(DocumentDto.TableName, "nodeId", "contentId")},
{syntax.ColumnWithAlias(ContentDto.TableName, "contentTypeId", "contentTypeId")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "userId", "userId")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "versionDate", "versionDate")},
{syntax.ColumnWithAlias(DocumentVersionDto.TableName ,"published", "currentPublishedVersion")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "current", "currentDraftVersion")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "preventCleanup", "preventCleanup")},
{syntax.ColumnWithAlias(UserDto.TableName, "userName", "username")}
";

    /// <inheritdoc />
    public IReadOnlyCollection<ContentVersionCleanupPolicySettings> GetCleanupPolicies()
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return [];
        }

        Sql<ISqlContext> query = _scopeAccessor.AmbientScope.SqlContext.Sql();

        query.Select<ContentVersionCleanupPolicyDto>()
            .From<ContentVersionCleanupPolicyDto>();

        return _scopeAccessor.AmbientScope.Database.Fetch<ContentVersionCleanupPolicySettings>(query);
    }

    /// <inheritdoc />
    public IEnumerable<ContentVersionMeta> GetPagedItemsByContentId(int contentId, long pageIndex, int pageSize, out long totalRecords, int? languageId = null)
    {
        IScope? ambientScope = _scopeAccessor.AmbientScope;
        if (ambientScope is null)
        {
            totalRecords = 0;
            return [];
        }

        ISqlSyntaxProvider syntax = ambientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> query = ambientScope.SqlContext.Sql()
            .Select(GetQuotedSelectColumns(syntax))
            .From<DocumentDto>()
            .InnerJoin<ContentDto>()
            .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
            .LeftJoin<UserDto>()
            .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
            .LeftJoin<ContentVersionCultureVariationDto>()
            .On<ContentVersionCultureVariationDto, ContentVersionDto>(left => left.VersionId, right => right.Id)
            .Where<ContentVersionDto>(x => x.NodeId == contentId);

        // TODO: If there's not a better way to write this then we need a better way to write this.
        query = languageId.HasValue
            ? query.Where<ContentVersionCultureVariationDto>(x => x.LanguageId == languageId.Value)
            : query.WhereNull<ContentVersionCultureVariationDto>(x => x.LanguageId);

        query = query.OrderByDescending<ContentVersionDto>(x => x.Id);

        Page<ContentVersionMeta> page =
            ambientScope.Database.Page<ContentVersionMeta>(pageIndex + 1, pageSize, query);

        totalRecords = page.TotalItems;

        List<ContentVersionMeta> results = page.Items;
        EnsureUtcDates(results);
        return results;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Inserts version IDs into a temp table, then deletes from all related tables
    ///     using a subquery join against the temp table. This is significantly faster than
    ///     batched <c>WHERE IN (...)</c> for large sets, as the database engine can use
    ///     indexed joins against the temp table's primary key.
    /// </remarks>
    public void DeleteVersions(IEnumerable<int> versionIds)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return;
        }

        var allIds = versionIds.ToList();
        if (allIds.Count == 0)
        {
            return;
        }

        IDatabase db = _scopeAccessor.AmbientScope.Database;
        ISqlSyntaxProvider syntax = _scopeAccessor.AmbientScope.SqlContext.SqlSyntax;

        var tempTableName = "umbVersionsToDelete";
        var quotedTempTableName = syntax.TempTableName(tempTableName);
        try
        {
            db.Execute(syntax.CreateTempTable(tempTableName, $"{syntax.GetQuotedColumnName("Id")} INT NOT NULL PRIMARY KEY"));

            // Batch insert IDs into the temp table.
            foreach (IEnumerable<int> group in allIds.InGroupsOf(1000))
            {
                var batch = group.ToList();
                var placeholders = string.Join(",", batch.Select((_, i) => $"(@{i})"));
                db.Execute(
                    $"INSERT INTO {quotedTempTableName} ({syntax.GetQuotedColumnName("Id")}) VALUES {placeholders}",
                    batch.Cast<object>().ToArray());
            }

            // Delete from all related tables using a subquery against the temp table.
            db.Execute($"DELETE FROM {syntax.GetQuotedTableName(PropertyDataDto.TableName)} WHERE {syntax.GetQuotedColumnName("versionId")} IN (SELECT {syntax.GetQuotedColumnName("Id")} FROM {quotedTempTableName})");
            db.Execute($"DELETE FROM {syntax.GetQuotedTableName(ContentVersionCultureVariationDto.TableName)} WHERE {syntax.GetQuotedColumnName("versionId")} IN (SELECT {syntax.GetQuotedColumnName("Id")} FROM {quotedTempTableName})");
            db.Execute($"DELETE FROM {syntax.GetQuotedTableName(DocumentVersionDto.TableName)} WHERE {syntax.GetQuotedColumnName("id")} IN (SELECT {syntax.GetQuotedColumnName("Id")} FROM {quotedTempTableName})");
            db.Execute($"DELETE FROM {syntax.GetQuotedTableName(ContentVersionDto.TableName)} WHERE {syntax.GetQuotedColumnName("id")} IN (SELECT {syntax.GetQuotedColumnName("Id")} FROM {quotedTempTableName})");
        }
        finally
        {
            db.Execute(syntax.DropTempTable(tempTableName));
        }
    }

    /// <inheritdoc />
    public void SetPreventCleanup(int versionId, bool preventCleanup)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return;
        }

        Sql<ISqlContext>? query = _scopeAccessor.AmbientScope.SqlContext.Sql()
            .Update<ContentVersionDto>(x => x.Set(y => y.PreventCleanup, preventCleanup))
            .Where<ContentVersionDto>(x => x.Id == versionId);

        _scopeAccessor.AmbientScope?.Database.Execute(query);
    }

    /// <inheritdoc />
    public ContentVersionMeta? Get(int versionId)
    {
        IScope? ambientScope = _scopeAccessor.AmbientScope;
        if (ambientScope is null)
        {
            return null;
        }

        ISqlSyntaxProvider syntax = ambientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> query = ambientScope.SqlContext.Sql()
            .Select(GetQuotedSelectColumns(syntax))
            .From<DocumentDto>()
            .InnerJoin<ContentDto>()
            .On<DocumentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<DocumentVersionDto>()
            .On<ContentVersionDto, DocumentVersionDto>(left => left.Id, right => right.Id)
            .LeftJoin<UserDto>()
            .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
            .Where<ContentVersionDto>(x => x.Id == versionId);

        ContentVersionMeta result = ambientScope.Database.Single<ContentVersionMeta>(query);
        result.EnsureUtc();
        return result;
    }

    private static void EnsureUtcDates(IEnumerable<ContentVersionMeta> versions)
    {
        foreach (ContentVersionMeta version in versions)
        {
            version.EnsureUtc();
        }
    }
}
