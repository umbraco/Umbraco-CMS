using Microsoft.Extensions.DependencyInjection;
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
public class ServerRoleAwareLastSyncedRepository : ILastSyncedRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LastSyncedRepository _databaseRepository;
    private readonly FileSystemLastSyncedRepository _fileSystemRepository;

    private IServerRoleAccessor? _serverRoleAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerRoleAwareLastSyncedRepository"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to lazily resolve the server role accessor.</param>
    /// <param name="databaseRepository">The database repository for non-subscriber servers.</param>
    /// <param name="fileSystemRepository">The file system repository for subscriber servers.</param>
    public ServerRoleAwareLastSyncedRepository(
        IServiceProvider serviceProvider,
        LastSyncedRepository databaseRepository,
        FileSystemLastSyncedRepository fileSystemRepository)
    {
        _serviceProvider = serviceProvider;
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

    private ILastSyncedRepository GetRepository()
    {
        // We have to access the IServerRoleAccessor this way to prevent a deadlock. If IServerRoleAccessor is injected directly
        // it has circular dependencies that will cause a deadlock.
        _serverRoleAccessor ??= _serviceProvider.GetRequiredService<IServerRoleAccessor>();

        return _serverRoleAccessor.CurrentServerRole == ServerRole.Subscriber
            ? _fileSystemRepository
            : _databaseRepository;
    }
}
