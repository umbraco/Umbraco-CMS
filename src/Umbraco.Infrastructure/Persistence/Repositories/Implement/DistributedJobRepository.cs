using NPoco;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
internal class DistributedJobRepository(IScopeAccessor scopeAccessor) : IDistributedJobRepository
{
    /// <inheritdoc />
    public string? GetRunnable()
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.IsRunning == false)
            .OrderBy<DistributedJobDto>(x => x.LastRun);

        IUmbracoDatabase database = scopeAccessor.AmbientScope.Database;
        List<DistributedJobDto> jobs = database.Fetch<DistributedJobDto>(sql);

        // Grab the first runnable job.
        DistributedJobDto? job = jobs.FirstOrDefault(x => x.LastRun < DateTime.UtcNow - TimeSpan.FromTicks(x.Period));

        if (job is null)
        {
            return null;
        }

        job.LastAttemptedRun = DateTime.UtcNow;
        job.IsRunning = true;
        database.Update(job);


        return job.Name;
    }

    /// <inheritdoc />
    public void Finish(string jobName)
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return;
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.Name == jobName);

        IUmbracoDatabase database = scopeAccessor.AmbientScope.Database;
        DistributedJobDto? job = database.FirstOrDefault<DistributedJobDto>(sql);

        if (job is null)
        {
            return;
        }

        DateTime currentDateTime = DateTime.UtcNow;
        job.LastAttemptedRun = currentDateTime;
        job.LastRun = currentDateTime;
        job.IsRunning = false;
        database.Update(job);
    }

    /// <inheritdoc/>
    public IEnumerable<DistributedBackgroundJobModel> GetAll()
    {
        if (scopeAccessor.AmbientScope is null)
        {
            throw new PanicException("No scope, could not get distributed jobs");
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

    private DistributedJobDto MapToDto(DistributedBackgroundJobModel model) =>
        new()
        {
            Name = model.Name,
            Period = model.Period.Ticks,
            LastRun = model.LastRun,
            IsRunning = model.IsRunning,
            LastAttemptedRun = model.LastAttemptedRun,
        };

    private DistributedBackgroundJobModel MapFromDto(DistributedJobDto jobDto) =>
        new()
        {
            Name = jobDto.Name,
            Period = TimeSpan.FromTicks(jobDto.Period),
            LastRun = jobDto.LastRun,
            IsRunning = jobDto.IsRunning,
            LastAttemptedRun = jobDto.LastAttemptedRun,
        };
}
