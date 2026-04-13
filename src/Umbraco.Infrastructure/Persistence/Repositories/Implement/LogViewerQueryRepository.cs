using System.Data;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

internal sealed class LogViewerQueryRepository : EntityRepositoryBase<int, ILogViewerQuery>, ILogViewerQueryRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogViewerQueryRepository"/> class.
    /// </summary>
    /// <param name="scopeAccessor">The <see cref="IScopeAccessor"/> used to manage database scopes.</param>
    /// <param name="cache">The <see cref="AppCaches"/> instance for application-level caching.</param>
    /// <param name="logger">The <see cref="ILogger{LogViewerQueryRepository}"/> used for logging operations.</param>
    /// <param name="repositoryCacheVersionService">The <see cref="IRepositoryCacheVersionService"/> for managing repository cache versions.</param>
    /// <param name="cacheSyncService">The <see cref="ICacheSyncService"/> responsible for synchronizing cache across instances.</param>
    public LogViewerQueryRepository(
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<LogViewerQueryRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
    }

    /// <summary>Retrieves a log viewer query by its name.</summary>
    /// <param name="name">The name of the log viewer query to retrieve.</param>
    /// <returns>The log viewer query with the specified name, or null if not found.</returns>
    public ILogViewerQuery? GetByName(string name) =>

        // use the underlying GetAll which will force cache all log queries
        GetMany().FirstOrDefault(x => x.Name == name);

    protected override IRepositoryCachePolicy<ILogViewerQuery, int> CreateCachePolicy() =>
        new FullDataSetRepositoryCachePolicy<ILogViewerQuery, int>(GlobalIsolatedCache, ScopeAccessor,  RepositoryCacheVersionService, CacheSyncService, GetEntityId, /*expires:*/ false);

    protected override IEnumerable<ILogViewerQuery> PerformGetAll(params int[]? ids)
    {
        Sql<ISqlContext>? sql = GetBaseQuery(false).Where<LogViewerQueryDto>(c => c.Id > 0);
        if (ids?.Length > 0)
        {
            sql.WhereIn<LogViewerQueryDto>(c => c.Id, ids);
        }

        return Database.Fetch<LogViewerQueryDto>(sql).Select(ConvertFromDto);
    }

    protected override IEnumerable<ILogViewerQuery> PerformGetByQuery(IQuery<ILogViewerQuery> query) =>
        throw new NotSupportedException("This repository does not support this method");

    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        sql = isCount ? sql.SelectCount() : sql.Select<LogViewerQueryDto>();
        sql = sql.From<LogViewerQueryDto>();
        return sql;
    }

    protected override string GetBaseWhereClause() => $"{QuoteTableName(Constants.DatabaseSchema.Tables.LogViewerQuery)}.id = @id";

    protected override IEnumerable<string> GetDeleteClauses()
    {
        var list = new List<string> { $"DELETE FROM {QuoteTableName(Constants.DatabaseSchema.Tables.LogViewerQuery)} WHERE id = @id" };
        return list;
    }

    protected override void PersistNewItem(ILogViewerQuery entity)
    {
        Sql<ISqlContext> sql = Sql()
            .SelectCount()
            .From<LogViewerQueryDto>()
            .Where<LogViewerQueryDto>(x => x.Name == entity.Name);
        var exists = Database.ExecuteScalar<int>(sql);
        if (exists > 0)
        {
            throw new DuplicateNameException($"The log query name '{entity.Name}' is already used");
        }

        entity.AddingEntity();

        var factory = new LogViewerQueryModelFactory();
        LogViewerQueryDto dto = factory.BuildDto(entity);

        var id = Convert.ToInt32(Database.Insert(dto));
        entity.Id = id;
    }

    protected override void PersistUpdatedItem(ILogViewerQuery entity)
    {
        entity.UpdatingEntity();
        Sql<ISqlContext> sql = Sql()
            .SelectCount()
            .From<LogViewerQueryDto>()
            .Where<LogViewerQueryDto>(x => x.Name == entity.Name && x.Id != entity.Id);
        var exists = Database.ExecuteScalar<int>(sql);

        // ensure there is no other log query with the same name on another entity
        if (exists > 0)
        {
            throw new DuplicateNameException($"The log query name '{entity.Name}' is already used");
        }

        var factory = new LogViewerQueryModelFactory();
        LogViewerQueryDto dto = factory.BuildDto(entity);

        Database.Update(dto);
    }

    protected override ILogViewerQuery? PerformGet(int id) =>

        // use the underlying GetAll which will force cache all log queries
        GetMany().FirstOrDefault(x => x.Id == id);

    private static ILogViewerQuery ConvertFromDto(LogViewerQueryDto dto)
    {
        var factory = new LogViewerQueryModelFactory();
        ILogViewerQuery entity = factory.BuildEntity(dto);
        return entity;
    }

    internal sealed class LogViewerQueryModelFactory
    {
        /// <summary>
        /// Creates an <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.LogViewerQueryRepository.ILogViewerQuery" /> instance from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.LogViewerQueryRepository.LogViewerQueryDto" />.
        /// </summary>
        /// <param name="dto">The DTO containing the log viewer query information.</param>
        /// <returns>A new <see cref="Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.LogViewerQueryRepository.ILogViewerQuery" /> instance.</returns>
        public ILogViewerQuery BuildEntity(LogViewerQueryDto dto)
        {
            var logViewerQuery = new LogViewerQuery(dto.Name, dto.Query) { Id = dto.Id };
            return logViewerQuery;
        }

        /// <summary>
        /// Builds a <see cref="LogViewerQueryDto"/> from the given <see cref="ILogViewerQuery"/> entity.
        /// </summary>
        /// <param name="entity">The log viewer query entity to convert.</param>
        /// <returns>A data transfer object representing the log viewer query.</returns>
        public LogViewerQueryDto BuildDto(ILogViewerQuery entity)
        {
            var dto = new LogViewerQueryDto { Name = entity.Name, Query = entity.Query, Id = entity.Id };
            return dto;
        }
    }
}
