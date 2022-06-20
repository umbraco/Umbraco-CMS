using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal class ExternalLoginRepository : EntityRepositoryBase<int, IIdentityUserLogin>, IExternalLoginRepository, IExternalLoginWithKeyRepository
{
    public ExternalLoginRepository(IScopeAccessor scopeAccessor, AppCaches cache,
        ILogger<ExternalLoginRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    /// <inheritdoc />
    [Obsolete("Use method that takes guid as param")]
    public void DeleteUserLogins(int memberId) => DeleteUserLogins(memberId.ToGuid());

    /// <inheritdoc />
    [Obsolete("Use method that takes guid as param")]
    public void Save(int userId, IEnumerable<IExternalLogin> logins) => Save(userId.ToGuid(), logins);

    /// <inheritdoc />
    [Obsolete("Use method that takes guid as param")]
    public void Save(int userId, IEnumerable<IExternalLoginToken> tokens) => Save(userId.ToGuid(), tokens);

    /// <summary>
    ///     Query for user tokens
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public IEnumerable<IIdentityUserToken> Get(IQuery<IIdentityUserToken>? query)
    {
        Sql<ISqlContext> sqlClause = GetBaseTokenQuery(false);

        var translator = new SqlTranslator<IIdentityUserToken>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<ExternalLoginTokenDto> dtos = Database.Fetch<ExternalLoginTokenDto>(sql);

        foreach (ExternalLoginTokenDto dto in dtos)
        {
            yield return ExternalLoginFactory.BuildEntity(dto);
        }
    }

    /// <summary>
    ///     Count for user tokens
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public int Count(IQuery<IIdentityUserToken> query)
    {
        Sql<ISqlContext> sql = Sql().SelectCount().From<ExternalLoginDto>();
        return Database.ExecuteScalar<int>(sql);
    }

    /// <inheritdoc />
    public void DeleteUserLogins(Guid userOrMemberKey) =>
        Database.Delete<ExternalLoginDto>("WHERE userOrMemberKey=@userOrMemberKey", new { userOrMemberKey });

    /// <inheritdoc />
    public void Save(Guid userOrMemberKey, IEnumerable<IExternalLogin> logins)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<ExternalLoginDto>()
            .From<ExternalLoginDto>()
            .Where<ExternalLoginDto>(x => x.UserOrMemberKey == userOrMemberKey)
            .ForUpdate();

        // deduplicate the logins
        logins = logins.DistinctBy(x => x.ProviderKey + x.LoginProvider).ToList();

        var toUpdate = new Dictionary<int, IExternalLogin>();
        var toDelete = new List<int>();
        var toInsert = new List<IExternalLogin>(logins);

        List<ExternalLoginDto>? existingLogins = Database.Fetch<ExternalLoginDto>(sql);

        foreach (ExternalLoginDto? existing in existingLogins)
        {
            IExternalLogin? found = logins.FirstOrDefault(x =>
                x.LoginProvider.Equals(existing.LoginProvider, StringComparison.InvariantCultureIgnoreCase)
                && x.ProviderKey.Equals(existing.ProviderKey, StringComparison.InvariantCultureIgnoreCase));

            if (found != null)
            {
                toUpdate.Add(existing.Id, found);

                // if it's an update then it's not an insert
                toInsert.RemoveAll(x => x.ProviderKey == found.ProviderKey && x.LoginProvider == found.LoginProvider);
            }
            else
            {
                toDelete.Add(existing.Id);
            }
        }

        // do the deletes, updates and inserts
        if (toDelete.Count > 0)
        {
            Database.DeleteMany<ExternalLoginDto>().Where(x => toDelete.Contains(x.Id)).Execute();
        }

        foreach (KeyValuePair<int, IExternalLogin> u in toUpdate)
        {
            Database.Update(ExternalLoginFactory.BuildDto(userOrMemberKey, u.Value, u.Key));
        }

        Database.InsertBulk(toInsert.Select(i => ExternalLoginFactory.BuildDto(userOrMemberKey, i)));
    }

    /// <inheritdoc />
    public void Save(Guid userOrMemberKey, IEnumerable<IExternalLoginToken> tokens)
    {
        // get the existing logins (provider + id)
        var existingUserLogins = Database
            .Fetch<ExternalLoginDto>(GetBaseQuery(false)
                .Where<ExternalLoginDto>(x => x.UserOrMemberKey == userOrMemberKey))
            .ToDictionary(x => x.LoginProvider, x => x.Id);

        // deduplicate the tokens
        tokens = tokens.DistinctBy(x => x.LoginProvider + x.Name).ToList();

        var providers = tokens.Select(x => x.LoginProvider).Distinct().ToList();

        Sql<ISqlContext> sql = GetBaseTokenQuery(true)
            .WhereIn<ExternalLoginDto>(x => x.LoginProvider, providers)
            .Where<ExternalLoginDto>(x => x.UserOrMemberKey == userOrMemberKey);

        var toUpdate = new Dictionary<int, (IExternalLoginToken externalLoginToken, int externalLoginId)>();
        var toDelete = new List<int>();
        var toInsert = new List<IExternalLoginToken>(tokens);

        List<ExternalLoginTokenDto>? existingTokens = Database.Fetch<ExternalLoginTokenDto>(sql);

        foreach (ExternalLoginTokenDto existing in existingTokens)
        {
            IExternalLoginToken? found = tokens.FirstOrDefault(x =>
                x.LoginProvider.InvariantEquals(existing.ExternalLoginDto.LoginProvider)
                && x.Name.InvariantEquals(existing.Name));

            if (found != null)
            {
                toUpdate.Add(existing.Id, (found, existing.ExternalLoginId));

                // if it's an update then it's not an insert
                toInsert.RemoveAll(x =>
                    x.LoginProvider.InvariantEquals(found.LoginProvider) && x.Name.InvariantEquals(found.Name));
            }
            else
            {
                toDelete.Add(existing.Id);
            }
        }

        // do the deletes, updates and inserts
        if (toDelete.Count > 0)
        {
            Database.DeleteMany<ExternalLoginTokenDto>().Where(x => toDelete.Contains(x.Id)).Execute();
        }

        foreach (KeyValuePair<int, (IExternalLoginToken externalLoginToken, int externalLoginId)> u in toUpdate)
        {
            Database.Update(ExternalLoginFactory.BuildDto(u.Value.externalLoginId, u.Value.externalLoginToken, u.Key));
        }

        var insertDtos = new List<ExternalLoginTokenDto>();
        foreach (IExternalLoginToken t in toInsert)
        {
            if (!existingUserLogins.TryGetValue(t.LoginProvider, out var externalLoginId))
            {
                throw new InvalidOperationException(
                    $"A token was attempted to be saved for login provider {t.LoginProvider} which is not assigned to this user");
            }

            insertDtos.Add(ExternalLoginFactory.BuildDto(externalLoginId, t));
        }

        Database.InsertBulk(insertDtos);
    }

    protected override IIdentityUserLogin? PerformGet(int id)
    {
        Sql<ISqlContext> sql = GetBaseQuery(false);
        sql.Where(GetBaseWhereClause(), new { id });

        ExternalLoginDto? dto = Database.Fetch<ExternalLoginDto>(SqlSyntax.SelectTop(sql, 1)).FirstOrDefault();
        if (dto == null)
        {
            return null;
        }

        IIdentityUserLogin entity = ExternalLoginFactory.BuildEntity(dto);

        // reset dirty initial properties (U4-1946)
        entity.ResetDirtyProperties(false);

        return entity;
    }

    protected override IEnumerable<IIdentityUserLogin> PerformGetAll(params int[]? ids)
    {
        if (ids?.Any() ?? false)
        {
            return PerformGetAllOnIds(ids);
        }

        Sql<ISqlContext> sql = GetBaseQuery(false).OrderByDescending<ExternalLoginDto>(x => x.CreateDate);

        return ConvertFromDtos(Database.Fetch<ExternalLoginDto>(sql))
            .ToArray(); // we don't want to re-iterate again!
    }

    protected override IEnumerable<IIdentityUserLogin> PerformGetByQuery(IQuery<IIdentityUserLogin> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IIdentityUserLogin>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();

        List<ExternalLoginDto>? dtos = Database.Fetch<ExternalLoginDto>(sql);

        foreach (ExternalLoginDto? dto in dtos)
        {
            yield return ExternalLoginFactory.BuildEntity(dto);
        }
    }

    private IEnumerable<IIdentityUserLogin> PerformGetAllOnIds(params int[] ids)
    {
        if (ids.Any() == false)
        {
            yield break;
        }

        foreach (var id in ids)
        {
            IIdentityUserLogin? identityUserLogin = Get(id);
            if (identityUserLogin is not null)
            {
                yield return identityUserLogin;
            }
        }
    }

    private IEnumerable<IIdentityUserLogin> ConvertFromDtos(IEnumerable<ExternalLoginDto> dtos)
    {
        foreach (IIdentityUserLogin entity in dtos.Select(ExternalLoginFactory.BuildEntity))
        {
            // reset dirty initial properties (U4-1946)
            ((BeingDirtyBase)entity).ResetDirtyProperties(false);

            yield return entity;
        }
    }

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        if (isCount)
        {
            sql.SelectCount();
        }
        else
        {
            sql.SelectAll();
        }

        sql.From<ExternalLoginDto>();
        return sql;
    }

    protected override string GetBaseWhereClause() => $"{Constants.DatabaseSchema.Tables.ExternalLogin}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { "DELETE FROM umbracoExternalLogin WHERE id = @id" };
        return list;
    }

    protected override void PersistNewItem(IIdentityUserLogin entity)
    {
        entity.AddingEntity();

        ExternalLoginDto dto = ExternalLoginFactory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;

        entity.ResetDirtyProperties();
    }

    protected override void PersistUpdatedItem(IIdentityUserLogin entity)
    {
        entity.UpdatingEntity();

        ExternalLoginDto dto = ExternalLoginFactory.BuildDto(entity);

        Database.Update(dto);

        entity.ResetDirtyProperties();
    }

    private Sql<ISqlContext> GetBaseTokenQuery(bool forUpdate)
        => forUpdate
            ? Sql()
                .Select<ExternalLoginTokenDto>(r => r.Select(x => x.ExternalLoginDto))
                .From<ExternalLoginTokenDto>()
                .AppendForUpdateHint() // ensure these table values are locked for updates, the ForUpdate ext method does not work here
                .InnerJoin<ExternalLoginDto>()
                .On<ExternalLoginTokenDto, ExternalLoginDto>(x => x.ExternalLoginId, x => x.Id)
            : Sql()
                .Select<ExternalLoginTokenDto>()
                .AndSelect<ExternalLoginDto>(x => x.LoginProvider, x => x.UserOrMemberKey)
                .From<ExternalLoginTokenDto>()
                .InnerJoin<ExternalLoginDto>()
                .On<ExternalLoginTokenDto, ExternalLoginDto>(x => x.ExternalLoginId, x => x.Id);
}
