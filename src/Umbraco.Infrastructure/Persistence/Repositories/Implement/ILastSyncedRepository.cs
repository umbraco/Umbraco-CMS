namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public interface ILastSyncedRepository
{
    Task<int?> GetInternal();

    Task<int?> GetExternal();

    Task SaveInternal(int id);

    Task SaveExternal(int id);
}
