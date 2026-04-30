using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class UserDataRepository : IUserDataRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserDataRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    public UserDataRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <summary>
    /// Asynchronously retrieves user data by the specified unique key.
    /// </summary>
    /// <param name="key">The unique identifier of the user data to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user data if found; otherwise, null.</returns>
    public async Task<IUserData?> GetAsync(Guid key)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .Select<UserDataDto>()
            .From<UserDataDto>()
            .Where<UserDataDto>(dataDto => dataDto.Key == key)
            .OrderBy<UserDataDto>(dto => dto.Identifier); // need to order to skiptake;

        UserDataDto? dto = await _scopeAccessor.AmbientScope.Database.FirstOrDefaultAsync<UserDataDto>(sql)!;

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

        // Fetching the total before applying OrderBy to avoid issue with count subquery.
        var totalItems = _scopeAccessor.AmbientScope?.Database.Count(sql!) ?? 0;

        sql = sql.OrderBy<UserDataDto>(dto => dto.Identifier); // need to order to skiptake

        List<UserDataDto>? userDataDtos =
            await _scopeAccessor.AmbientScope?.Database.SkipTakeAsync<UserDataDto>(skip, take, sql)!;

        return new PagedModel<IUserData> { Total = totalItems, Items = DtosToModels(userDataDtos) };
    }

    /// <summary>
    /// Asynchronously saves the specified user data to the database.
    /// </summary>
    /// <param name="userData">The user data instance to save.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the same <paramref name="userData"/> instance that was provided as input.</returns>
    public async Task<IUserData> Save(IUserData userData)
    {
        await _scopeAccessor.AmbientScope?.Database.InsertAsync(Map(userData))!;
        return userData;
    }

    /// <summary>
    /// Asynchronously updates the specified user data in the database.
    /// </summary>
    /// <param name="userData">The user data to update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the same <see cref="IUserData"/> instance provided as input.</returns>
    public async Task<IUserData> Update(IUserData userData)
    {
        await _scopeAccessor.AmbientScope?.Database.UpdateAsync(Map(userData))!;
        return userData;
    }

    /// <summary>
    /// Asynchronously deletes the specified user data from the database.
    /// </summary>
    /// <param name="userData">The user data entity to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task Delete(IUserData userData)
    {
        Sql<ISqlContext> sql = _scopeAccessor.AmbientScope!.Database.SqlContext.Sql()
            .Delete<UserDataDto>()
            .Where<UserDataDto>(dto => dto.Key == userData.Key);

        await _scopeAccessor.AmbientScope?.Database.ExecuteAsync(sql)!;
    }

    private static Sql<ISqlContext> ApplyFilter(Sql<ISqlContext> sql, IUserDataFilter filter)
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

    private static IEnumerable<IUserData> DtosToModels(IEnumerable<UserDataDto> dtos)
        => dtos.Select(Map);

    private static IUserData Map(UserDataDto dto)
        => new UserData
        {
            Key = dto.Key,
            Group = dto.Group,
            Identifier = dto.Identifier,
            Value = dto.Value,
            UserKey = dto.UserKey,
        };

    private static UserDataDto Map(IUserData userData)
        => new()
        {
            Key = userData.Key,
            Group = userData.Group,
            Identifier = userData.Identifier,
            Value = userData.Value,
            UserKey = userData.UserKey,
        };
}
