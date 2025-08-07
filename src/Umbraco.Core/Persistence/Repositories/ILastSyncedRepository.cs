namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ILastSyncedRepository
{
    Task<int?> GetInternal();

    Task<int?> GetExternal();

    Task SaveInternal(int id);

    Task SaveExternal(int id);
}
