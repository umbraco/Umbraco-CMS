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

    public LongRunningOperationRepository(
        IJsonSerializer jsonSerializer,
        IScopeAccessor scopeAccessor,
        AppCaches appCaches)
        : base(scopeAccessor, appCaches) =>
        _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public LongRunningOperation? Get(string type, Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Type == type && x.Id == id);

        LongRunningOperationDto dto = Database.FirstOrDefault<LongRunningOperationDto>(sql);
        return dto == null ? null : MapDtoToEntity(dto);
    }

    /// <inheritdoc/>
    public bool IsEnqueuedOrRunning(string type)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x =>
                x.Type == type && (x.Status == nameof(LongRunningOperationStatus.Enqueued) || x.Status == nameof(LongRunningOperationStatus.Running)));

        var count = Database.Count(sql);
        return count > 0;
    }

    /// <inheritdoc/>
    public bool IsEnqueuedOrRunning(string type, Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x =>
                x.Type == type
                && x.Id == id
                && (x.Status == nameof(LongRunningOperationStatus.Enqueued) || x.Status == nameof(LongRunningOperationStatus.Running)));

        var count = Database.Count(sql);
        return count > 0;
    }

    /// <inheritdoc/>
    public void Create(LongRunningOperation operation, TimeSpan expires)
    {
        LongRunningOperationDto dto = MapEntityToDto(operation, expires);
        Database.Insert(dto);
    }

    /// <inheritdoc/>
    public void UpdateStatus(string type, Guid id, LongRunningOperationStatus status, TimeSpan expires)
    {
        Sql<ISqlContext> sql = Sql()
            .Update<LongRunningOperationDto>(x => x
                .Set(y => y.Status, status.ToString())
                .Set(y => y.UpdateDate, DateTime.UtcNow)
                .Set(y => y.ExpireDate, DateTime.UtcNow + expires))
            .Where<LongRunningOperationDto>(x => x.Type == type && x.Id == id);

        Database.Execute(sql);
    }

    /// <inheritdoc/>
    public T? GetResult<T>(string type, Guid id)
    {
        Sql<ISqlContext> sql = Sql()
            .Select<LongRunningOperationDto>()
            .From<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.Type == type && x.Id == id);

        LongRunningOperationDto dto = Database.FirstOrDefault<LongRunningOperationDto>(sql);
        return dto?.Result == null ? default : _jsonSerializer.Deserialize<T>(dto.Result);
    }

    /// <inheritdoc/>
    public void SetResult<T>(string type, Guid id, T result, TimeSpan expires)
    {
        Sql<ISqlContext> sql = Sql()
            .Update<LongRunningOperationDto>(x => x
                .Set(y => y.Result, _jsonSerializer.Serialize(result))
                .Set(y => y.UpdateDate, DateTime.UtcNow)
                .Set(y => y.ExpireDate, DateTime.UtcNow + expires))
            .Where<LongRunningOperationDto>(x => x.Type == type && x.Id == id);

        Database.Execute(sql);
    }

    /// <inheritdoc/>
    public void CleanOperations(TimeSpan maxAgeOfOperations)
    {
        // Set operations that have expired and are still marked as enqueued or running to Failed status
        DateTime now = DateTime.UtcNow;
        Sql<ISqlContext> sql1 = Sql()
            .Update<LongRunningOperationDto>(x => x
                .Set(y => y.Status, nameof(LongRunningOperationStatus.Failed))
                .Set(y => y.UpdateDate, now))
            .Where<LongRunningOperationDto>(x =>
                x.ExpireDate < now
                && (x.Status == nameof(LongRunningOperationStatus.Enqueued) || x.Status == nameof(LongRunningOperationStatus.Running)));

        Database.Execute(sql1);

        // Delete operations that haven't been updated for longer than the specified max age
        DateTime olderThan = now - maxAgeOfOperations;
        Sql<ISqlContext> sql2 = Sql()
            .Delete<LongRunningOperationDto>()
            .Where<LongRunningOperationDto>(x => x.UpdateDate < olderThan);

        Database.Execute(sql2);
    }

    private static LongRunningOperation MapDtoToEntity(LongRunningOperationDto dto) =>
        new()
        {
            Id = dto.Id,
            Type = dto.Type,
            Status = dto.Status.EnumParse<LongRunningOperationStatus>(false),
        };

    private static LongRunningOperationDto MapEntityToDto(LongRunningOperation entity, TimeSpan expires) =>
        new()
        {
            Id = entity.Id,
            Type = entity.Type,
            Status = entity.Status.ToString(),
            CreateDate = DateTime.UtcNow,
            UpdateDate = DateTime.UtcNow,
            ExpireDate = DateTime.UtcNow + expires,
        };
}
