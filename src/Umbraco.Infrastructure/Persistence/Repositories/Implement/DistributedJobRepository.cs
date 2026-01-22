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

    /// <inheritdoc/>
    public void Add(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not add distributed jobs");
        }

        IEnumerable<DistributedJobDto> dtos = jobs.Select(MapToDto);
        scopeAccessor.AmbientScope.Database.InsertBulk(dtos);
    }

    /// <inheritdoc/>
    public void Delete(IEnumerable<DistributedBackgroundJobModel> jobs)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new InvalidOperationException("No scope, could not delete distributed jobs");
        }

        var jobIds = jobs.Select(x => x.Id).ToArray();
        if (jobIds.Length is 0)
        {
            return;
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Delete()
            .From<DistributedJobDto>()
            .WhereIn<DistributedJobDto>(x => x.Id, jobIds);

        scopeAccessor.AmbientScope.Database.Execute(sql);
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
