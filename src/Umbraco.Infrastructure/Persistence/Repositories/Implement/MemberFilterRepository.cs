// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
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

    private ISqlSyntaxProvider SqlSyntax =>
        _scopeAccessor.AmbientScope?.SqlContext.SqlSyntax
        ?? throw new NotSupportedException("Need to be executed in a scope.");

    private string QTab(string tableName) => SqlSyntax.GetQuotedTableName(tableName);
    private string QCol(string columnName) => SqlSyntax.GetQuotedColumnName(columnName);
    private string QName(string name) => SqlSyntax.GetQuotedName(name);

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
                n.{QCol(NodeDto.KeyColumnName)} AS {QName("key")},
                m.{QCol("Email")},
                m.{QCol("LoginName")} AS {QName("userName")},
                n.{QCol(NodeDto.TextColumnName)} AS {QName("name")},
                m.{QCol("isApproved")},
                m.{QCol("isLockedOut")},
                m.{QCol("lastLoginDate")},
                m.{QCol("lastLockoutDate")},
                m.{QCol("lastPasswordChangeDate")},
                CAST(0 AS bit) AS {QName("isExternalOnly")},
                ctn.{QCol("uniqueId")} AS {QName("memberTypeKey")},
                ctn.{QCol("text")} AS {QName("memberTypeName")},
                ctd.{QCol("icon")} AS {QName("memberTypeIcon")}
            FROM {QTab(MemberDto.TableName)} m
            INNER JOIN {QTab(NodeDto.TableName)} n ON n.{QCol(NodeDto.PrimaryKeyColumnName)} = m.{QCol(MemberDto.PrimaryKeyColumnName)}
            INNER JOIN {QTab(ContentDto.TableName)} ct ON ct.{QCol(ContentDto.PrimaryKeyColumnName)} = n.{QCol(NodeDto.PrimaryKeyColumnName)}
            INNER JOIN {QTab(NodeDto.TableName)} ctn ON ctn.{QCol(NodeDto.PrimaryKeyColumnName)} = ct.{QCol(ContentDto.ContentTypeIdColumnName)}
            INNER JOIN {QTab(ContentTypeDto.TableName)} ctd ON ctd.{QCol(ContentTypeDto.NodeIdColumnName)} = ctn.{QCol(NodeDto.PrimaryKeyColumnName)}");

        // Append optional JOINs before any WHERE clauses.
        if (filter.MemberGroupName.IsNullOrWhiteSpace() is false)
        {
            sql = sql.Append(
                $@"INNER JOIN {QTab(Member2MemberGroupDto.TableName)} m2mg
                    ON m2mg.{QCol(Member2MemberGroupDto.MemberColumnName)} = m.{QCol(MemberDto.PrimaryKeyColumnName)}
                INNER JOIN {QTab(NodeDto.TableName)} mgn
                    ON mgn.{QCol(NodeDto.PrimaryKeyColumnName)} = m2mg.{QCol(Member2MemberGroupDto.MemberGroupColumnName)}
                    AND mgn.{QCol(NodeDto.TextColumnName)} = @groupName", new { groupName = filter.MemberGroupName });
        }

        if (filter.MemberTypeId.HasValue)
        {
            sql = sql.Append($"WHERE ctn.{QCol("uniqueId")} = @typeId", new { typeId = filter.MemberTypeId.Value });
        }

        AppendWhereFilters(ref sql, filter, $"m.{QCol("Email")}", $"m.{QCol("LoginName")}", $"n.{QCol("text")}", $"m.{QCol("isApproved")}", $"m.{QCol("isLockedOut")}", filter.MemberTypeId.HasValue);

        return sql;
    }

    private Sql<ISqlContext> BuildExternalMemberSql(MemberFilter filter)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Append($@"SELECT
                em.{QCol("key")},
                em.{QCol("email")},
                em.{QCol("userName")},
                em.{QCol("name")},
                em.{QCol("isApproved")},
                em.{QCol("isLockedOut")},
                em.{QCol("lastLoginDate")},
                em.{QCol("lastLockoutDate")},
                NULL AS {QName("lastPasswordChangeDate")},
                CAST(1 AS bit) AS {QName("isExternalOnly")},
                NULL AS {QName("memberTypeKey")},
                NULL AS {QName("memberTypeName")},
                NULL AS {QName("memberTypeIcon")}
            FROM {QTab(ExternalMemberDto.TableName)} em");

        if (filter.MemberGroupName.IsNullOrWhiteSpace() is false)
        {
            sql = sql.Append(
                $@"INNER JOIN {QTab(ExternalMember2MemberGroupDto.TableName)} em2mg
                    ON em2mg.{QCol(ExternalMember2MemberGroupDto.ExternalMemberColumnName)} = em.{QCol(ExternalMemberDto.PrimaryKeyColumnName)}
                INNER JOIN {QTab(NodeDto.TableName)} emgn
                    ON emgn.{QCol(NodeDto.PrimaryKeyColumnName)} = em2mg.{QCol(ExternalMember2MemberGroupDto.MemberGroupColumnName)}
                    AND emgn.{QCol(NodeDto.TextColumnName)} = @groupName", new { groupName = filter.MemberGroupName });
        }

        AppendWhereFilters(ref sql, filter, $"em.{QCol("email")}", $"em.{QCol("userName")}", $"em.{QCol("name")}", $"em.{QCol("isApproved")}", $"em.{QCol("isLockedOut")}", hasWhereAlready: false);

        return sql;
    }

    private void AppendWhereFilters(
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

    private string MapOrderByColumn(string? orderBy) =>
        orderBy?.ToLowerInvariant() switch
        {
            "email" => QCol("email"),
            "name" => QCol("name"),
            "isapproved" => QCol("isApproved"),
            "islockedout" => QCol("isLockedOut"),
            "lastlogindate" => QCol("lastLoginDate"),
            _ => QCol("userName"),
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
        [Column("key")]
        public Guid Key { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("userName")]
        public string UserName { get; set; } = string.Empty;

        [Column("name")]
        public string? Name { get; set; }

        [Column("isApproved")]
        public bool IsApproved { get; set; }

        [Column("isLockedOut")]
        public bool IsLockedOut { get; set; }

        [Column("lastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [Column("lastLockoutDate")]
        public DateTime? LastLockoutDate { get; set; }

        [Column("lastPasswordChangeDate")]
        public DateTime? LastPasswordChangeDate { get; set; }

        [Column("isExternalOnly")]
        public bool IsExternalOnly { get; set; }

        [Column("memberTypeKey")]
        public Guid? MemberTypeKey { get; set; }

        [Column("memberTypeName")]
        public string? MemberTypeName { get; set; }

        [Column("memberTypeIcon")]
        public string? MemberTypeIcon { get; set; }
    }
}
