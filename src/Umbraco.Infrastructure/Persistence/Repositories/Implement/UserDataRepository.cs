using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class UserDataRepository : IUserDataRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public UserDataRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    public async Task<IUserData?> GetAsync(Guid key)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<UserDataDto>()
            .From<UserDataDto>()
            .Where<UserDataDto>(dataDto => dataDto.Key == key)
            .OrderBy<UserDataDto>(dto => dto.Identifier); // need to order to skiptake;

        UserDataDto? dto = await _scopeAccessor.AmbientScope?.Database.FirstOrDefaultAsync<UserDataDto>(sql)!;

        return dto is null ? null : Map(dto);
    }

    public async Task<PagedModel<IUserData>> GetAsync(int skip, int take, IUserDataFilter? filter = null)
    {
        Sql<ISqlContext>? sql = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .Select<UserDataDto>()
            .From<UserDataDto>();

        if (sql is null)
        {
            return new PagedModel<IUserData>();
        }

        if (filter is not null)
        {
            sql = ApplyFilter(sql, filter);
        }

        sql = sql.OrderBy<UserDataDto>(dto => dto.Identifier); // need to order to skiptake

        List<UserDataDto>? userDataDtos =
            await _scopeAccessor.AmbientScope?.Database.SkipTakeAsync<UserDataDto>(skip, take, sql)!;

        var totalItems = _scopeAccessor.AmbientScope?.Database.Count(sql!) ?? 0;

        return new PagedModel<IUserData> { Total = totalItems, Items = DtosToModels(userDataDtos) };
    }

    public async Task<IUserData> Save(IUserData userData)
    {
        await _scopeAccessor.AmbientScope?.Database.InsertAsync(Map(userData))!;
        return userData;
    }

    public async Task<IUserData> Update(IUserData userData)
    {
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(Map(userData))!;
        return userData;
    }

    public async Task Delete(IUserData userData)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<UserDataDto>()
            .Where<UserDataDto>(dto => dto.Key == userData.Key);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;
    }

    private Sql<ISqlContext> ApplyFilter(Sql<ISqlContext> sql, IUserDataFilter filter)
    {
        if (filter.Groups?.Count > 0)
        {
            sql = sql.Where<UserDataDto>(dto => filter.Groups.Contains(dto.Group));
        }

        if (filter.Identifiers?.Count > 0)
        {
            sql = sql.Where<UserDataDto>(dto => filter.Identifiers.Contains(dto.Identifier));
        }

        if (filter.UserKeys?.Count > 0)
        {
            sql = sql.Where<UserDataDto>(dto => filter.UserKeys.Contains(dto.UserKey));
        }

        return sql;
    }

    private IEnumerable<IUserData> DtosToModels(IEnumerable<UserDataDto> dtos)
        => dtos.Select(Map);

    private IUserData Map(UserDataDto dto)
        => new UserData
        {
            Key = dto.Key,
            Group = dto.Group,
            Identifier = dto.Identifier,
            Value = dto.Value,
            UserKey = dto.UserKey,
        };

    private UserDataDto Map(IUserData userData)
        => new()
        {
            Key = userData.Key,
            Group = userData.Group,
            Identifier = userData.Identifier,
            Value = userData.Value,
            UserKey = userData.UserKey,
        };
}
