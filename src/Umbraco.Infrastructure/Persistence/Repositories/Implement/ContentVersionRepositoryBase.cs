using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal abstract class ContentVersionRepositoryBase<TContentDto, TContentVersionDto>
    where TContentDto : INodeDto
    where TContentVersionDto : IContentVersionDto
{
    private readonly IScopeAccessor _scopeAccessor;

    protected abstract string ContentDtoTableName { get; }

    protected abstract string ContentVersionDtoTableName { get; }

    public ContentVersionRepositoryBase(IScopeAccessor scopeAccessor) =>
        _scopeAccessor = scopeAccessor ?? throw new ArgumentNullException(nameof(scopeAccessor));

    /// <inheritdoc />
    /// <remarks>
    ///     Never includes current draft version. <br />
    ///     Never includes current published version.<br />
    ///     Never includes versions marked as "preventCleanup".<br />
    /// </remarks>
    public IReadOnlyCollection<ContentVersionMeta> GetContentVersionsEligibleForCleanup()
    {
        IScope? ambientScope = _scopeAccessor.AmbientScope;
        if (ambientScope is null)
        {
            return [];
        }

        ISqlSyntaxProvider syntax = ambientScope.SqlContext.SqlSyntax;
        Sql<ISqlContext> query = ambientScope.SqlContext.Sql()
            .Select(GetQuotedSelectColumns(syntax))
            .From<TContentDto>()
            .InnerJoin<ContentDto>()
            .On<TContentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<TContentVersionDto>()
            .On<ContentVersionDto, TContentVersionDto>(left => left.Id, right => right.Id)
            .LeftJoin<UserDto>()
            .On<UserDto, ContentVersionDto>(left => left.Id, right => right.UserId)
            .Where<ContentVersionDto>(x => !x.Current) // Never delete current draft version
            .Where<ContentVersionDto>(x => !x.PreventCleanup) // Never delete "pinned" versions
            .Where<TContentVersionDto>(x => !x.Published); // Never delete published version

        List<ContentVersionMeta> results = ambientScope.Database.Fetch<ContentVersionMeta>(query);
        EnsureUtcDates(results);
        return results;
    }

    private string GetQuotedSelectColumns(ISqlSyntaxProvider syntax) =>
        $@"
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "id", "versionId")},
{syntax.ColumnWithAlias(ContentDtoTableName, "nodeId", "contentId")},
{syntax.ColumnWithAlias(ContentDto.TableName, "contentTypeId", "contentTypeId")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "userId", "userId")},
{syntax.ColumnWithAlias(ContentVersionDto.TableName, "versionDate", "versionDate")},
{syntax.ColumnWithAlias(ContentVersionDtoTableName,"published", "currentPublishedVersion")},
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
            .From<TContentDto>()
            .InnerJoin<ContentDto>()
            .On<TContentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<TContentVersionDto>()
            .On<ContentVersionDto, TContentVersionDto>(left => left.Id, right => right.Id)
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
    ///     Deletes in batches of <see cref="Constants.Sql.MaxParameterCount" />
    /// </remarks>
    public void DeleteVersions(IEnumerable<int> versionIds)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return;
        }

        foreach (IEnumerable<int> group in versionIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            var groupedVersionIds = group.ToList();

            /* Note: We had discussed doing this in a single SQL Command.
            *  If you can work out how to make that work with SQL CE, let me know!
            *  Can use test PerformContentVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive to try things out.
            */

            Sql<ISqlContext> query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                .Delete<PropertyDataDto>()
                .WhereIn<PropertyDataDto>(x => x.VersionId, groupedVersionIds);
            _scopeAccessor.AmbientScope.Database.Execute(query);

            query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                .Delete<ContentVersionCultureVariationDto>()
                .WhereIn<ContentVersionCultureVariationDto>(x => x.VersionId, groupedVersionIds);
            _scopeAccessor.AmbientScope.Database.Execute(query);

            query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                .Delete<TContentVersionDto>()
                .WhereIn<TContentVersionDto>(x => x.Id, groupedVersionIds);
            _scopeAccessor.AmbientScope.Database.Execute(query);

            query = _scopeAccessor.AmbientScope.SqlContext.Sql()
                .Delete<ContentVersionDto>()
                .WhereIn<ContentVersionDto>(x => x.Id, groupedVersionIds);
            _scopeAccessor.AmbientScope.Database.Execute(query);
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
            .From<TContentDto>()
            .InnerJoin<ContentDto>()
            .On<TContentDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentVersionDto>()
            .On<ContentDto, ContentVersionDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<TContentVersionDto>()
            .On<ContentVersionDto, TContentVersionDto>(left => left.Id, right => right.Id)
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
