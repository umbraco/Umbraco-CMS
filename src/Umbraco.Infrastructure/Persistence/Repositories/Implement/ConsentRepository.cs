using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the NPoco implementation of <see cref="IConsentRepository" />.
/// </summary>
internal class ConsentRepository : EntityRepositoryBase<int, IConsent>, IConsentRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConsentRepository" /> class.
    /// </summary>
    public ConsentRepository(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<ConsentRepository> logger)
        : base(scopeAccessor, cache, logger)
    {
    }

    /// <inheritdoc />
    public void ClearCurrent(string source, string context, string action)
    {
        Sql<ISqlContext> sql = Sql()
            .Update<ConsentDto>(u => u.Set(x => x.Current, false))
            .Where<ConsentDto>(x => x.Source == source && x.Context == context && x.Action == action && x.Current);
        Database.Execute(sql);
    }

    /// <inheritdoc />
    protected override IConsent PerformGet(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override IEnumerable<IConsent> PerformGetAll(params int[]? ids) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override IEnumerable<IConsent> PerformGetByQuery(IQuery<IConsent> query)
    {
        Sql<ISqlContext> sqlClause = Sql().Select<ConsentDto>().From<ConsentDto>();
        var translator = new SqlTranslator<IConsent>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate().OrderByDescending<ConsentDto>(x => x.CreateDate);
        return ConsentFactory.BuildEntities(Database.Fetch<ConsentDto>(sql));
    }

    /// <inheritdoc />
    protected override Sql<ISqlContext> GetBaseQuery(bool isCount) => throw new NotSupportedException();

    /// <inheritdoc />
    protected override string GetBaseWhereClause() => throw new NotSupportedException();

    /// <inheritdoc />
    protected override IEnumerable<string> GetDeleteClauses() => throw new NotSupportedException();

    /// <inheritdoc />
    protected override void PersistNewItem(IConsent entity)
    {
        entity.AddingEntity();

        ConsentDto dto = ConsentFactory.BuildDto(entity);
        Database.Insert(dto);
        entity.Id = dto.Id;
        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override void PersistUpdatedItem(IConsent entity)
    {
        entity.UpdatingEntity();

        ConsentDto dto = ConsentFactory.BuildDto(entity);
        Database.Update(dto);
        entity.ResetDirtyProperties();

        IsolatedCache.Clear(RepositoryCacheKeys.GetKey<IConsent, int>(entity.Id));
    }
}
