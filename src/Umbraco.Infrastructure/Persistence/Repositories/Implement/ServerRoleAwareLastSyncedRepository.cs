using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// A decorator around <see cref="ILastSyncedRepository"/> that delegates to either a database
/// or file system repository based on the server role.
/// </summary>
/// <remarks>
/// Subscriber servers cannot write to the database, so they use a file-system
/// implementation instead. All other roles use the database implementation.
/// </remarks>
internal sealed class ServerRoleAwareLastSyncedRepository : ILastSyncedRepository
{
    private readonly Lazy<IServerRoleAccessor> _serverRoleAccessor;
    private readonly LastSyncedRepository _databaseRepository;
    private readonly FileSystemLastSyncedRepository _fileSystemRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRoleAwareLastSyncedRepository"/> class.
    /// </summary>
    /// <param name="serverRoleAccessor">Lazy accessor for the server role, deferred to avoid a singleton resolution deadlock.</param>
    /// <param name="databaseRepository">The database repository for non-subscriber servers.</param>
    /// <param name="fileSystemRepository">The file system repository for subscriber servers.</param>
    public ServerRoleAwareLastSyncedRepository(
        Lazy<IServerRoleAccessor> serverRoleAccessor,
        LastSyncedRepository databaseRepository,
        FileSystemLastSyncedRepository fileSystemRepository)
    {
        _serverRoleAccessor = serverRoleAccessor;
        _databaseRepository = databaseRepository;
        _fileSystemRepository = fileSystemRepository;
    }

    /// <inheritdoc />
    public Task<int?> GetInternalIdAsync() => GetRepository().GetInternalIdAsync();

    /// <inheritdoc />
    public Task<int?> GetExternalIdAsync() => GetRepository().GetExternalIdAsync();

    /// <inheritdoc />
    public Task SaveInternalIdAsync(int id) => GetRepository().SaveInternalIdAsync(id);

    /// <inheritdoc />
    public Task SaveExternalIdAsync(int id) => GetRepository().SaveExternalIdAsync(id);

    /// <inheritdoc />
    public Task DeleteEntriesOlderThanAsync(DateTime pruneDate) => GetRepository().DeleteEntriesOlderThanAsync(pruneDate);

    private ILastSyncedRepository GetRepository() =>
        _serverRoleAccessor.Value.CurrentServerRole == ServerRole.Subscriber
            ? _fileSystemRepository
            : _databaseRepository;
}
