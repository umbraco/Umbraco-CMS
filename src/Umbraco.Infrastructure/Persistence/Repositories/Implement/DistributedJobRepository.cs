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

        long cutoffTicks = DateTimeOffset.Now.Ticks;

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where("lastRun + period < @0", cutoffTicks);

        DistributedJobDto? job = scopeAccessor.AmbientScope.Database.FirstOrDefault<DistributedJobDto>(sql);


        return job?.Name;
    }
}
