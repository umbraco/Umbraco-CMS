using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class DistributedJobRepository(IScopeAccessor scopeAccessor) : IDistributedJobRepository
{
    public string? GetRunnableJob()
    {
        return null;
    }
}
