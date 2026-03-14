// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Implements persistence operations for external-only members that are not backed by the content system.
/// </summary>
internal sealed class ExternalMemberRepository : IExternalMemberRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExternalMemberRepository" /> class.
    /// </summary>
    public ExternalMemberRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database =>
        _scopeAccessor.AmbientScope?.Database
        ?? throw new NotSupportedException("Need to be executed in a scope.");

    private ISqlContext SqlContext =>
        _scopeAccessor.AmbientScope?.SqlContext
        ?? throw new NotSupportedException("Need to be executed in a scope.");

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByKeyAsync(Guid key)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<ExternalMemberDto>()
            .From<ExternalMemberDto>()
            .Where<ExternalMemberDto>(x => x.Key == key);

        ExternalMemberDto? dto = await Database.FirstOrDefaultAsync<ExternalMemberDto>(sql);
        return dto is null ? null : MapToIdentity(dto);
    }

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByEmailAsync(string email)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<ExternalMemberDto>()
            .From<ExternalMemberDto>()
            .Where<ExternalMemberDto>(x => x.Email == email);

        ExternalMemberDto? dto = await Database.FirstOrDefaultAsync<ExternalMemberDto>(sql);
        return dto is null ? null : MapToIdentity(dto);
    }

    /// <inheritdoc />
    public async Task<ExternalMemberIdentity?> GetByUsernameAsync(string username)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<ExternalMemberDto>()
            .From<ExternalMemberDto>()
            .Where<ExternalMemberDto>(x => x.UserName == username);

        ExternalMemberDto? dto = await Database.FirstOrDefaultAsync<ExternalMemberDto>(sql);
        return dto is null ? null : MapToIdentity(dto);
    }

    /// <inheritdoc />
    public async Task<PagedModel<ExternalMemberIdentity>> GetPagedAsync(int skip, int take)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<ExternalMemberDto>()
            .From<ExternalMemberDto>()
            .OrderBy<ExternalMemberDto>(x => x.UserName);

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        Page<ExternalMemberDto> page = await Database.PageAsync<ExternalMemberDto>(pageNumber + 1, pageSize, sql);

        return new PagedModel<ExternalMemberIdentity>
        {
            Total = page.TotalItems,
            Items = page.Items.Select(MapToIdentity),
        };
    }

    /// <inheritdoc />
    public async Task<int> CreateAsync(ExternalMemberIdentity member)
    {
        ExternalMemberDto dto = MapToDto(member);
        var result = await Database.InsertAsync(dto);
        var id = Convert.ToInt32(result);
        member.Id = id;
        return id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(ExternalMemberIdentity member)
    {
        ExternalMemberDto dto = MapToDto(member);
        await Database.UpdateAsync(dto);
    }

    /// <inheritdoc />
    public async Task UpdateLoginTimestampAsync(Guid memberKey, DateTime lastLoginDate, string securityStamp)
    {
        // Direct SQL for the login fast path — no entity load, minimal overhead.
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Append(
                $"UPDATE {ExternalMemberDto.TableName} SET lastLoginDate = @lastLoginDate, securityStamp = @securityStamp WHERE [key] = @key",
                new { lastLoginDate, securityStamp, key = memberKey });

        await Database.ExecuteAsync(sql);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid key)
    {
        // Delete group memberships first (FK constraint).
        ExternalMemberDto? dto = await Database.FirstOrDefaultAsync<ExternalMemberDto>(
            SqlContext.Sql()
                .Select<ExternalMemberDto>()
                .From<ExternalMemberDto>()
                .Where<ExternalMemberDto>(x => x.Key == key));

        if (dto is null)
        {
            return;
        }

        Database.DeleteMany<ExternalMember2MemberGroupDto>()
            .Where(x => x.ExternalMemberId == dto.Id)
            .Execute();

        await Database.DeleteAsync(dto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetRolesAsync(Guid memberKey)
    {
        Sql<ISqlContext> sql = SqlContext.Sql()
            .Select<NodeDto>(x => x.Text)
            .From<ExternalMember2MemberGroupDto>()
            .InnerJoin<NodeDto>()
            .On<ExternalMember2MemberGroupDto, NodeDto>(
                left => left.MemberGroupId, right => right.NodeId)
            .InnerJoin<ExternalMemberDto>()
            .On<ExternalMember2MemberGroupDto, ExternalMemberDto>(
                left => left.ExternalMemberId, right => right.Id)
            .Where<ExternalMemberDto>(x => x.Key == memberKey)
            .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.MemberGroup);

        List<NodeDto> nodes = await Database.FetchAsync<NodeDto>(sql);

        return nodes
            .Where(n => n.Text is not null)
            .Select(n => n.Text!);
    }

    /// <inheritdoc />
    public async Task AssignRolesAsync(int externalMemberId, int[] memberGroupIds)
    {
        foreach (var groupId in memberGroupIds)
        {
            var dto = new ExternalMember2MemberGroupDto
            {
                ExternalMemberId = externalMemberId,
                MemberGroupId = groupId,
            };

            await Database.InsertAsync(dto);
        }
    }

    /// <inheritdoc />
    public async Task RemoveRolesAsync(int externalMemberId, int[] memberGroupIds)
    {
        Database.DeleteMany<ExternalMember2MemberGroupDto>()
            .Where(x => x.ExternalMemberId == externalMemberId && memberGroupIds.Contains(x.MemberGroupId))
            .Execute();
    }

    private static ExternalMemberIdentity MapToIdentity(ExternalMemberDto dto) =>
        new()
        {
            Id = dto.Id,
            Key = dto.Key,
            Email = dto.Email,
            UserName = dto.UserName,
            Name = dto.Name,
            IsApproved = dto.IsApproved,
            IsLockedOut = dto.IsLockedOut,
            LastLoginDate = dto.LastLoginDate,
            LastLockoutDate = dto.LastLockoutDate,
            CreateDate = dto.CreateDate,
            SecurityStamp = dto.SecurityStamp,
            ProfileData = dto.ProfileData,
        };

    private static ExternalMemberDto MapToDto(ExternalMemberIdentity identity) =>
        new()
        {
            Id = identity.Id,
            Key = identity.Key,
            Email = identity.Email,
            UserName = identity.UserName,
            Name = identity.Name,
            IsApproved = identity.IsApproved,
            IsLockedOut = identity.IsLockedOut,
            LastLoginDate = identity.LastLoginDate,
            LastLockoutDate = identity.LastLockoutDate,
            CreateDate = identity.CreateDate,
            SecurityStamp = identity.SecurityStamp,
            ProfileData = identity.ProfileData,
        };
}
