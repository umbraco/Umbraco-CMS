using NPoco;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
internal class DistributedJobRepository(IScopeAccessor scopeAccessor) : IDistributedJobRepository
{
    /// <inheritdoc/>
    public DistributedBackgroundJobModel? GetByName(string jobName)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not get distributed jobs");
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.Name == jobName);

        DistributedJobDto? dto = scopeAccessor.AmbientScope.Database.FirstOrDefault<DistributedJobDto>(sql);
        return dto is null ? null : MapFromDto(dto);
    }

    /// <inheritdoc/>
    public IEnumerable<DistributedBackgroundJobModel> GetAll()
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not get distributed jobs");
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>();

        IUmbracoDatabase database = scopeAccessor.AmbientScope.Database;
        List<DistributedJobDto> jobs = database.Fetch<DistributedJobDto>(sql);
        return jobs.Select(MapFromDto);
    }

    /// <inheritdoc/>
    public void Update(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return;
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        scopeAccessor.AmbientScope.Database.Update(dto);
    }

    /// <inheritdoc/>
    public void Add(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not add distributed job");
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        scopeAccessor.AmbientScope.Database.Insert(dto);
    }

    /// <inheritdoc/>
    public void Delete(DistributedBackgroundJobModel distributedBackgroundJob)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not delete distributed job");
        }

        DistributedJobDto dto = MapToDto(distributedBackgroundJob);

        int rowsAffected = scopeAccessor.AmbientScope.Database.Delete(dto);
        if (rowsAffected == 0)
        {
            throw new InvalidOperationException("Could not delete distributed job, it may have already been deleted");
        }
    }

    private DistributedJobDto MapToDto(DistributedBackgroundJobModel model) =>
        new()
        {
            Id = model.Id,
            Name = model.Name,
            Period = model.Period.Ticks,
            LastRun = model.LastRun,
            IsRunning = model.IsRunning,
            LastAttemptedRun = model.LastAttemptedRun,
        };

    private DistributedBackgroundJobModel MapFromDto(DistributedJobDto jobDto) =>
        new()
        {
            Id = jobDto.Id,
            Name = jobDto.Name,
            Period = TimeSpan.FromTicks(jobDto.Period),
            LastRun = jobDto.LastRun,
            IsRunning = jobDto.IsRunning,
            LastAttemptedRun = jobDto.LastAttemptedRun,
        };
}
