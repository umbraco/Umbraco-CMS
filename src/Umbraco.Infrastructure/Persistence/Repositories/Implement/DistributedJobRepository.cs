using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class DistributedJobRepository(IScopeAccessor scopeAccessor) : IDistributedJobRepository
{
    public string? GetRunnableJob()
    {
        if (scopeAccessor.AmbientScope is null)
        {
            return null;
        }

        Sql<ISqlContext> sql = scopeAccessor.AmbientScope.SqlContext.Sql()
            .Select<DistributedJobDto>()
            .From<DistributedJobDto>()
            .Where<DistributedJobDto>(x => x.LastRun > DateTime.Now.AddMinutes(-5));


        return null;
    }
}
