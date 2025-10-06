using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <inheritdoc />
public class DistributedJobRepository(IScopeAccessor scopeAccessor) : IDistributedJobRepository
{
    /// <inheritdoc />
    public string? GetRunnableJob()
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
    public void FinishJob(string jobName)
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
}
