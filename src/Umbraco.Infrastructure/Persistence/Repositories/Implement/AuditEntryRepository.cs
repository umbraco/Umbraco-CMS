using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Represents the NPoco implementation of <see cref="IAuditEntryRepository" />.
/// </summary>
internal sealed class AuditEntryRepository : EntityRepositoryBase<int, IAuditEntry>, IAuditEntryRepository
{
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditEntryRepository" /> class.
    /// </summary>
    public AuditEntryRepository(
        IRuntimeState runtimeState,
        IScopeAccessor scopeAccessor,
        AppCaches cache,
        ILogger<AuditEntryRepository> logger,
        IRepositoryCacheVersionService repositoryCacheVersionService,
        ICacheSyncService cacheSyncService)
        : base(
            scopeAccessor,
            cache,
            logger,
            repositoryCacheVersionService,
            cacheSyncService)
    {
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    public IEnumerable<IAuditEntry> GetPage(long pageIndex, int pageCount, out long records)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<AuditEntryDto>()
            .From<AuditEntryDto>()
            .OrderByDescending<AuditEntryDto>(x => x.EventDate);

        Page<AuditEntryDto> page = Database.Page<AuditEntryDto>(pageIndex + 1, pageCount, sql);
        records = page.TotalItems;
        return page.Items.Select(AuditEntryFactory.BuildEntity);
    }

    /// <inheritdoc />
    protected override IAuditEntry? PerformGet(int id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<AuditEntryDto>()
            .From<AuditEntryDto>()
            .Where<AuditEntryDto>(x => x.Id == id);

        AuditEntryDto? dto = Database.FirstOrDefault<AuditEntryDto>(sql);
        return dto == null ? null : AuditEntryFactory.BuildEntity(dto);
    }

    /// <inheritdoc />
    protected override IEnumerable<IAuditEntry> PerformGetAll(params int[]? ids)
    {
        if (ids?.Length == 0)
        {
            Sql<ISqlContext> sql = Sql()
                .Select<AuditEntryDto>()
                .From<AuditEntryDto>();

            return Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity);
        }

        var entries = new List<IAuditEntry>();

        foreach (IEnumerable<int> group in ids.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Sql<ISqlContext> sql = Sql()
                .Select<AuditEntryDto>()
                .From<AuditEntryDto>()
                .WhereIn<AuditEntryDto>(x => x.Id, group);

            entries.AddRange(Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity));
        }

        return entries;
    }

    /// <inheritdoc />
    protected override IEnumerable<IAuditEntry> PerformGetByQuery(IQuery<IAuditEntry> query)
    {
        Sql<ISqlContext> sqlClause = GetBaseQuery(false);
        var translator = new SqlTranslator<IAuditEntry>(sqlClause, query);
        Sql<ISqlContext> sql = translator.Translate();
        return Database.Fetch<AuditEntryDto>(sql).Select(AuditEntryFactory.BuildEntity);
    }

    /// <inheritdoc />
    protected override Sql<ISqlContext> GetBaseQuery(bool isCount)
    {
        Sql<ISqlContext> sql = Sql();
        sql = isCount ? sql.SelectCount() : sql.Select<AuditEntryDto>();
        sql = sql.From<AuditEntryDto>();
        return sql;
    }

    /// <inheritdoc />
    protected override string GetBaseWhereClause() => $"{QuoteTableName(AuditEntryDto.TableName)}.id = @id";

    /// <inheritdoc />
    protected override IEnumerable<string> GetDeleteClauses() =>
        throw new NotSupportedException("Audit entries cannot be deleted.");

    /// <inheritdoc />
    protected override void PersistNewItem(IAuditEntry entity)
    {
        entity.AddingEntity();

        AuditEntryDto dto = AuditEntryFactory.BuildDto(entity);
        try
        {
            Database.Insert(dto);
        }
        catch (DbException) when (_runtimeState.Level == RuntimeLevel.Upgrade)
        {
            // This can happen when in upgrade state, before the migration to add user keys runs.
            // In this case, we will try to insert the audit entry without the user keys.
            // TODO (V22): Remove this catch clause when 'V_17_0_0.AddGuidsToAuditEntries' is removed.
            Expression<Func<AuditEntryDto, object?>>[] fields =
            [
                x => x.PerformingUserId,
                x => x.PerformingDetails,
                x => x.PerformingIp,
                x => x.EventDate,
                x => x.AffectedUserId,
                x => x.AffectedDetails,
                x => x.EventType,
                x => x.EventDetails
            ];

            var cols = Sql().ColumnsForInsert(fields);
            IEnumerable<object?> values = fields.Select(f => f.Compile().Invoke(dto));

            Sql<ISqlContext> sqlValues = Sql();
            foreach (var (value, index) in values.Select((v, i) => (v, i)))
            {
                switch (value)
                {
                    case null:
                        sqlValues.Append((index == 0 ? string.Empty : ",") + "NULL");
                        break;
                    case "":
                        sqlValues.Append((index == 0 ? string.Empty : ",") + "''");
                        break;
                    default:
                        sqlValues.Append((index == 0 ? string.Empty : ",") + "@0", value);
                        break;
                }
            }

            Sql<ISqlContext> sqlInsert = Sql($"INSERT INTO {QuoteTableName(AuditEntryDto.TableName)} ({cols})")
                .Append("VALUES (")
                .Append(sqlValues)
                .Append(")");

            Database.Execute(sqlInsert);
        }

        entity.Id = dto.Id;
        entity.ResetDirtyProperties();
    }

    /// <inheritdoc />
    protected override void PersistUpdatedItem(IAuditEntry entity) =>
        throw new NotSupportedException("Audit entries cannot be updated.");
}
