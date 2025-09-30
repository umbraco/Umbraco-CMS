namespace Umbraco.Cms.Infrastructure.Persistence.Repositories;

public interface IDistributedJobRepository
{
    /// <summary>
    /// Gets the next runnable job from the database, if there are no jobs to run, get a null value.
    /// </summary>
    /// <returns></returns>
    string? GetRunnableJob();
}
