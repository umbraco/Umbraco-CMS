using System.Globalization;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// A file system based implementation of <see cref="ILastSyncedRepository"/> for use on
/// subscriber servers that cannot write to the database.
/// </summary>
internal sealed class FileSystemLastSyncedRepository : ILastSyncedRepository
{
    private readonly IHostingEnvironment _hostingEnvironment;

    private string? _distCacheFolder;
    private string? _internalFilePath;
    private string? _externalFilePath;

    private int? _lastInternalId;
    private int? _lastExternalId;
    private bool _lastInternalIdReady;
    private bool _lastExternalIdReady;
    private object? _internalLock;
    private object? _externalLock;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemLastSyncedRepository"/> class.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment used to resolve file paths.</param>
    public FileSystemLastSyncedRepository(IHostingEnvironment hostingEnvironment)
        => _hostingEnvironment = hostingEnvironment;

    /// <inheritdoc />
    public Task<int?> GetInternalIdAsync()
    {
        EnsureInternalInitialized();
        return Task.FromResult(_lastInternalId);
    }

    /// <inheritdoc />
    public Task<int?> GetExternalIdAsync()
    {
        EnsureExternalInitialized();
        return Task.FromResult(_lastExternalId);
    }

    /// <inheritdoc />
    public Task SaveInternalIdAsync(int id)
    {
        EnsureInternalInitialized();

        lock (_internalLock!)
        {
            WriteIdToFile(InternalFilePath, id);
            _lastInternalId = id;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveExternalIdAsync(int id)
    {
        EnsureExternalInitialized();

        lock (_externalLock!)
        {
            WriteIdToFile(ExternalFilePath, id);
            _lastExternalId = id;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteEntriesOlderThanAsync(DateTime pruneDate) => Task.CompletedTask;

    private void EnsureInternalInitialized()
        => LazyInitializer.EnsureInitialized(
            ref _lastInternalId,
            ref _lastInternalIdReady,
            ref _internalLock,
            () => ReadIdFromFile(InternalFilePath));

    private void EnsureExternalInitialized()
        => LazyInitializer.EnsureInitialized(
            ref _lastExternalId,
            ref _lastExternalIdReady,
            ref _externalLock,
            () => ReadIdFromFile(ExternalFilePath));

    private string InternalFilePath => LazyInitializer.EnsureInitialized(
        ref _internalFilePath,
        () => Path.Combine(EnsureDistCacheFolder(), GetFileHash() + "-lastsynced-internal.txt"));

    private string ExternalFilePath => LazyInitializer.EnsureInitialized(
        ref _externalFilePath,
        () => Path.Combine(EnsureDistCacheFolder(), GetFileHash() + "-lastsynced-external.txt"));

    private string EnsureDistCacheFolder() => LazyInitializer.EnsureInitialized(ref _distCacheFolder, () =>
    {
        var folder = Path.Combine(_hostingEnvironment.LocalTempPath, "DistCache");

        if (Directory.Exists(folder) == false)
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    });

    private string GetFileHash()
        => (Environment.MachineName + _hostingEnvironment.ApplicationId).GenerateHash();

    private int? ReadIdFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            if (int.TryParse(content, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
            {
                return id;
            }
        }

        return null;
    }

    private void WriteIdToFile(string filePath, int id)
        => File.WriteAllText(filePath, id.ToString(CultureInfo.InvariantCulture));
}
