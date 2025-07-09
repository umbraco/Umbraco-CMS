using NPoco;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Repository for managing long-running operations.
/// </summary>
internal class LongRunningOperationRepository : RepositoryBase, ILongRunningOperationRepository
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongRunningOperationRepository"/> class.
    /// </summary>
    public LongRunningOperationRepository(
        IJsonSerializer jsonSerializer,
        IScopeAccessor scopeAccessor,
        AppCaches appCaches,
        TimeProvider timeProvider)
        : base(scopeAccessor, appCaches)
    {
        _jsonSerializer = jsonSerializer;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public void Create(LongRunningOperation operation, DateTimeOffset expirationDate)
    {
        LongRunningOperationDto dto = MapEntityToDto(operation, expirationDate);
        Database.Insert(dto);
    }

    /// <inheritdoc />
    public LongRunningOperation? Get(Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Id == id);

        LongRunningOperationDto dto = Database.FirstOrDefault<LongRunningOperationDto>(sql);
        return dto == null ? null : MapDtoToEntity(dto);
    }

    /// <inheritdoc />
    public LongRunningOperation<T>? Get<T>(Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Id == id);

        LongRunningOperationDto dto = Database.FirstOrDefault<LongRunningOperationDto>(sql);
        return dto == null ? null : MapDtoToEntity<T>(dto);
    }

    /// <inheritdoc/>
    public IEnumerable<LongRunningOperation> GetByType(string type, LongRunningOperationStatus[] statuses)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Type == type);

        if (statuses.Length > 0)
        {
            bool includeStale = statuses.Contains(LongRunningOperationStatus.Stale);
            string[] possibleStaleStatuses = [nameof(LongRunningOperationStatus.Enqueued), nameof(LongRunningOperationStatus.Running)];
            IEnumerable<string> statusList = statuses.Except([LongRunningOperationStatus.Stale]).Select(s => s.ToString());

            DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
            sql = sql.Where<LongRunningOperationDto>(x =>
                (statusList.Contains(x.Status) && (!possibleStaleStatuses.Contains(x.Status) || x.ExpirationDate >= now))
                || (includeStale && possibleStaleStatuses.Contains(x.Status) && x.ExpirationDate < now));
        }

        IEnumerable<LongRunningOperationDto> dtos = Database.Query<LongRunningOperationDto>(sql);
        return dtos.Select(MapDtoToEntity);
    }

    /// <inheritdoc/>
    public LongRunningOperationStatus? GetStatus(Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>(x => x.Status)
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Id == id);

        return Database.ExecuteScalar<string>(sql)?.EnumParse<LongRunningOperationStatus>(false);
    }

    /// <inheritdoc/>
    public void UpdateStatus(Guid id, LongRunningOperationStatus status, DateTimeOffset expirationTime)
    {
        Sql<ISqlContext> sql = Sql()
            .Update<LongRunningOperationDto>(x => x
                .Set(y => y.Status, status.ToString())
                .Set(y => y.UpdateDate, DateTime.UtcNow)
                .Set(y => y.ExpirationDate, expirationTime.DateTime))
            .Where<LongRunningOperationDto>(x => x.Id == id);

        Database.Execute(sql);
    }

    /// <inheritdoc/>
    public void SetResult<T>(Guid id, T result)
    {
        Sql<ISqlContext> sql = Sql()
            .Update<LongRunningOperationDto>(x => x
                .Set(y => y.Result, _jsonSerializer.Serialize(result))
                .Set(y => y.UpdateDate, DateTime.UtcNow))
            .Where<LongRunningOperationDto>(x => x.Id == id);

        Database.Execute(sql);
    }

    /// <inheritdoc/>
    public void CleanOperations(DateTimeOffset olderThan)
    {
        Sql<ISqlContext> sql2 = Sql()
            .Delete<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.UpdateDate < olderThan);

        Database.Execute(sql2);
    }

    private LongRunningOperation MapDtoToEntity(LongRunningOperationDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = dto.Type,
            Status = DetermineStatus(dto),
        };

    private static LongRunningOperationDto MapEntityToDto(LongRunningOperation entity, DateTimeOffset expirationTime) =>
        new()
        {
            Id = entity.Id,
            Type = entity.Type,
            Status = entity.Status.ToString(),
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            ExpirationDate = expirationTime.UtcDateTime,
        };

    private LongRunningOperation<T> MapDtoToEntity<T>(LongRunningOperationDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = dto.Type,
            Status = DetermineStatus(dto),
            Result = dto.Result == null ? default : _jsonSerializer.Deserialize<T>(dto.Result),
        };

    private LongRunningOperationStatus DetermineStatus(LongRunningOperationDto dto)
    {
        LongRunningOperationStatus status = dto.Status.EnumParse<LongRunningOperationStatus>(false);
        DateTimeOffset now = _timeProvider.GetUtcNow();
        if (status is LongRunningOperationStatus.Enqueued or LongRunningOperationStatus.Running
            && now.UtcDateTime >= dto.ExpirationDate)
        {
            status = LongRunningOperationStatus.Stale;
        }

        return status;
    }
}
