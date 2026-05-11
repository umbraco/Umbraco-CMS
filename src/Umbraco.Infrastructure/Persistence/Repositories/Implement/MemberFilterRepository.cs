// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Provides combined, paginated member queries across both the content member store
///     and the external member store using a UNION query.
/// </summary>
internal sealed class MemberFilterRepository : IMemberFilterRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemberFilterRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">The scope accessor.</param>
    public MemberFilterRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database =>
        _scopeAccessor.AmbientScope?.Database
        ?? throw new NotSupportedException("Need to be executed in a scope.");

    private ISqlContext SqlContext =>
        _scopeAccessor.AmbientScope?.SqlContext
        ?? throw new NotSupportedException("Need to be executed in a scope.");

    /// <inheritdoc />
    public async Task<PagedModel<MemberFilterItem>> GetPagedByFilterAsync(MemberFilter filter, int skip, int take, Ordering ordering)
    {
        // Build the content members SELECT.
        Sql<ISqlContext> contentSql = BuildContentMemberSql(filter);

        // Build the external members SELECT (excluded when filtering by memberTypeId since external members have no type).
        Sql<ISqlContext>? externalSql = filter.MemberTypeId.HasValue ? null : BuildExternalMemberSql(filter);

        // Combine with UNION ALL inside a subquery for unified ordering and paging.
        Sql<ISqlContext> combinedSql;
        if (externalSql is not null)
        {
            combinedSql = SqlContext.Sql()
                .Append("SELECT * FROM (")
                .Append(contentSql)
                .Append("UNION ALL")
                .Append(externalSql)
                .Append(") AS combined");
        }
        else
        {
            combinedSql = SqlContext.Sql()
                .Append("SELECT * FROM (")
                .Append(contentSql)
                .Append(") AS combined");
        }

        // Apply ordering.
        var orderColumn = MapOrderByColumn(ordering.OrderBy);
        combinedSql = ordering.Direction == Direction.Ascending
            ? combinedSql.Append($"ORDER BY {orderColumn} ASC")
            : combinedSql.Append($"ORDER BY {orderColumn} DESC");

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        Page<MemberFilterItemDto> page = await Database.PageAsync<MemberFilterItemDto>(pageNumber + 1, pageSize, combinedSql);

        return new PagedModel<MemberFilterItem>(
            page.TotalItems,
            page.Items.Select(MapToItem));
    }

    private Sql<ISqlContext> BuildContentMemberSql(MemberFilter filter)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Append($@"SELECT
                n.[uniqueId] AS [Key],
                m.[Email],
                m.[LoginName] AS [UserName],
                n.[text] AS [Name],
                m.[IsApproved],
                m.[IsLockedOut],
                m.[LastLoginDate],
                m.[LastLockoutDate],
                m.[LastPasswordChangeDate],
                CAST(0 AS bit) AS [IsExternalOnly],
                ctn.[uniqueId] AS [MemberTypeKey],
                ctn.[text] AS [MemberTypeName],
                ctd.[icon] AS [MemberTypeIcon]
            FROM [{Constants.DatabaseSchema.Tables.Member}] m
            INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] n ON n.[id] = m.[nodeId]
            INNER JOIN [{Constants.DatabaseSchema.Tables.Content}] ct ON ct.[nodeId] = n.[id]
            INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] ctn ON ctn.[id] = ct.[contentTypeId]
            INNER JOIN [cmsContentType] ctd ON ctd.[nodeId] = ctn.[id]");

        // Append optional JOINs before any WHERE clauses.
        if (filter.MemberGroupName.IsNullOrWhiteSpace() is false)
        {
            sql = sql.Append(
                $@"INNER JOIN [{Constants.DatabaseSchema.Tables.Member2MemberGroup}] m2mg ON m2mg.[{Member2MemberGroupDto.MemberColumnName}] = m.[nodeId]
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] mgn ON mgn.[id] = m2mg.[MemberGroup] AND mgn.[text] = @groupName", new { groupName = filter.MemberGroupName });
        }

        if (filter.MemberTypeId.HasValue)
        {
            sql = sql.Append("WHERE ctn.[uniqueId] = @typeId", new { typeId = filter.MemberTypeId.Value });
        }

        AppendWhereFilters(ref sql, filter, "m.[Email]", "m.[LoginName]", "n.[text]", "m.[IsApproved]", "m.[IsLockedOut]", filter.MemberTypeId.HasValue);

        return sql;
    }

    private Sql<ISqlContext> BuildExternalMemberSql(MemberFilter filter)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Append($@"SELECT
                em.[key] AS [Key],
                em.[email] AS [Email],
                em.[userName] AS [UserName],
                em.[name] AS [Name],
                em.[isApproved] AS [IsApproved],
                em.[isLockedOut] AS [IsLockedOut],
                em.[lastLoginDate] AS [LastLoginDate],
                em.[lastLockoutDate] AS [LastLockoutDate],
                CAST(NULL AS datetime) AS [LastPasswordChangeDate],
                CAST(1 AS bit) AS [IsExternalOnly],
                CAST(NULL AS uniqueidentifier) AS [MemberTypeKey],
                CAST(NULL AS nvarchar(255)) AS [MemberTypeName],
                CAST(NULL AS nvarchar(255)) AS [MemberTypeIcon]
            FROM [{Constants.DatabaseSchema.Tables.ExternalMember}] em");

        if (filter.MemberGroupName.IsNullOrWhiteSpace() is false)
        {
            sql = sql.Append(
                $@"INNER JOIN [{Constants.DatabaseSchema.Tables.ExternalMember2MemberGroup}] em2mg ON em2mg.[externalMemberId] = em.[id]
                INNER JOIN [{Constants.DatabaseSchema.Tables.Node}] emgn ON emgn.[id] = em2mg.[memberGroupId] AND emgn.[text] = @groupName", new { groupName = filter.MemberGroupName });
        }

        AppendWhereFilters(ref sql, filter, "em.[email]", "em.[userName]", "em.[name]", "em.[isApproved]", "em.[isLockedOut]", hasWhereAlready: false);

        return sql;
    }

    private static void AppendWhereFilters(
        ref Sql<ISqlContext> sql,
        MemberFilter filter,
        string emailCol,
        string usernameCol,
        string nameCol,
        string isApprovedCol,
        string isLockedOutCol,
        bool hasWhereAlready)
    {
        var hasWhere = hasWhereAlready;

        if (filter.IsApproved is not null)
        {
            sql = hasWhere
                ? sql.Append($"AND {isApprovedCol} = @approved", new { approved = filter.IsApproved.Value })
                : sql.Append($"WHERE {isApprovedCol} = @approved", new { approved = filter.IsApproved.Value });
            hasWhere = true;
        }

        if (filter.IsLockedOut is not null)
        {
            sql = hasWhere
                ? sql.Append($"AND {isLockedOutCol} = @lockedOut", new { lockedOut = filter.IsLockedOut.Value })
                : sql.Append($"WHERE {isLockedOutCol} = @lockedOut", new { lockedOut = filter.IsLockedOut.Value });
            hasWhere = true;
        }

        if (filter.Filter.IsNullOrWhiteSpace() is false)
        {
            var keyword = hasWhere ? "AND" : "WHERE";
            sql = sql.Append(
                $"{keyword} ({emailCol} LIKE @filterParam OR {usernameCol} LIKE @filterParam OR {nameCol} LIKE @filterParam)",
                new { filterParam = $"%{filter.Filter}%" });
        }
    }

    private static string MapOrderByColumn(string? orderBy) =>
        orderBy?.ToLowerInvariant() switch
        {
            "email" => "[Email]",
            "name" => "[Name]",
            "isapproved" => "[IsApproved]",
            "islockedout" => "[IsLockedOut]",
            "lastlogindate" => "[LastLoginDate]",
            _ => "[UserName]",
        };

    private static MemberFilterItem MapToItem(MemberFilterItemDto dto) =>
        new()
        {
            Key = dto.Key,
            Email = dto.Email,
            UserName = dto.UserName,
            Name = dto.Name,
            IsApproved = dto.IsApproved,
            IsLockedOut = dto.IsLockedOut,
            LastLoginDate = dto.LastLoginDate,
            LastLockoutDate = dto.LastLockoutDate,
            LastPasswordChangeDate = dto.LastPasswordChangeDate,
            IsExternalOnly = dto.IsExternalOnly,
            Kind = dto.IsExternalOnly ? MemberKind.ExternalOnly : MemberKind.Default,
            MemberTypeKey = dto.MemberTypeKey,
            MemberTypeName = dto.MemberTypeName,
            MemberTypeIcon = dto.MemberTypeIcon,
        };

    /// <summary>
    ///     Internal DTO for the UNION query result rows.
    /// </summary>
    [ExplicitColumns]
    private sealed class MemberFilterItemDto
    {
        [Column("Key")]
        public Guid Key { get; set; }

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("UserName")]
        public string UserName { get; set; } = string.Empty;

        [Column("Name")]
        public string? Name { get; set; }

        [Column("IsApproved")]
        public bool IsApproved { get; set; }

        [Column("IsLockedOut")]
        public bool IsLockedOut { get; set; }

        [Column("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [Column("LastLockoutDate")]
        public DateTime? LastLockoutDate { get; set; }

        [Column("LastPasswordChangeDate")]
        public DateTime? LastPasswordChangeDate { get; set; }

        [Column("IsExternalOnly")]
        public bool IsExternalOnly { get; set; }

        [Column("MemberTypeKey")]
        public Guid? MemberTypeKey { get; set; }

        [Column("MemberTypeName")]
        public string? MemberTypeName { get; set; }

        [Column("MemberTypeIcon")]
        public string? MemberTypeIcon { get; set; }
    }
}
