using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class TwoFactorLoginRepository : EntityRepositoryBase<int, ITwoFactorLogin>, ITwoFactorLoginRepository
{
    public TwoFactorLoginRepository(IScopeAccessor scopeAccessor, AppCaches cache,
        ILogger<TwoFactorLoginRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    public async Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey) =>
        await DeleteUserLoginsAsync(userOrMemberKey, null);

    public async Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey, string? providerName)
    {
        Sql<ISqlContext> sql = Sql()
            .Delete()
            .From<TwoFactorLoginDto>()
            .Where<TwoFactorLoginDto>(x => x.UserOrMemberKey == userOrMemberKey);

        if (providerName is not null)
        {
            sql = sql.Where<TwoFactorLoginDto>(x => x.ProviderName == providerName);
        }

        var deletedRows = await Database.ExecuteAsync(sql);

        return deletedRows > 0;
    }

    public async Task<IEnumerable<ITwoFactorLogin>> GetByUserOrMemberKeyAsync(Guid userOrMemberKey)
    {
        try
        {
            Sql<ISqlContext> sql = Sql()
                .Select<TwoFactorLoginDto>()
                .From<TwoFactorLoginDto>()
                .Where<TwoFactorLoginDto>(x => x.UserOrMemberKey == userOrMemberKey);
            List<TwoFactorLoginDto>? dtos = await Database.FetchAsync<TwoFactorLoginDto>(sql);
            return dtos.WhereNotNull().Select(Map).WhereNotNull();
        }

        // TODO (v11): Remove this as the table should always exist when upgrading from 10.x
        // SQL Server - table doesn't exist, likely upgrading from < 9.3.0.
        catch (SqlException ex) when (ex.Number == 208 && ex.Message.Contains(TwoFactorLoginDto.TableName))
        {
            return Enumerable.Empty<ITwoFactorLogin>();
        }
    }


    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = SqlContext.Sql();

        sql = isCount
            ? sql.SelectCount()
            : sql.Select<TwoFactorLoginDto>();

        sql.From<TwoFactorLoginDto>();

        return sql;
    }

    protected override string GetBaseWhereClause() =>
        Constants.DatabaseSchema.Tables.TwoFactorLogin + ".id = @id";

    protected override IEnumerable<string> GetDeleteClauses() => Enumerable.Empty<string>();

    protected override ITwoFactorLogin? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).Where<TwoFactorLoginDto>(x => x.Id == id);
        TwoFactorLoginDto? dto = Database.Fetch<TwoFactorLoginDto>(sql).FirstOrDefault();
        return dto == null ? null : Map(dto);
    }

    protected override IEnumerable<ITwoFactorLogin> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false).WhereIn<TwoFactorLoginDto>(x => x.Id, ids);
        List<TwoFactorLoginDto>? dtos = Database.Fetch<TwoFactorLoginDto>(sql);
        return dtos.WhereNotNull().Select(Map).WhereNotNull();
    }

    protected override IEnumerable<ITwoFactorLogin> PerformGetByQuery(IQuery<ITwoFactorLogin> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<ITwoFactorLogin>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        return Database.Fetch<TwoFactorLoginDto>(sql).Select(Map).WhereNotNull();
    }

    protected override void PersistNewItem(ITwoFactorLogin entity)
    {
        TwoFactorLoginDto? dto = Map(entity);
        Database.Insert(dto);
    }

    protected override void PersistUpdatedItem(ITwoFactorLogin entity)
    {
        TwoFactorLoginDto? dto = Map(entity);
        if (dto is not null)
        {
            Database.Update(dto);
        }
    }

    private static TwoFactorLoginDto? Map(ITwoFactorLogin entity)
    {
        if (entity == null)
        {
            return null;
        }

        return new TwoFactorLoginDto
        {
            Id = entity.Id,
            UserOrMemberKey = entity.UserOrMemberKey,
            ProviderName = entity.ProviderName,
            Secret = entity.Secret
        };
    }

    private static ITwoFactorLogin? Map(TwoFactorLoginDto dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new TwoFactorLogin
        {
            Id = dto.Id, UserOrMemberKey = dto.UserOrMemberKey, ProviderName = dto.ProviderName, Secret = dto.Secret
        };
    }
}
