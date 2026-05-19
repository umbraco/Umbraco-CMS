// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Used to cleanup temporary file locations.
/// </summary>
/// <remarks>
///     Will run on all servers - even though file upload should only be handled on the scheduling publisher, this will
///     ensure that in the case it happens on subscribers that they are cleaned up too.
/// </remarks>
public class TempFileCleanupJob : RecurringBackgroundJobBase
{
    /// <summary>
    /// Gets the time interval between each execution of the temporary file cleanup job.
    /// </summary>
    public override TimeSpan Period => TimeSpan.FromMinutes(60);

    /// <summary>
    /// Gets the server roles on which this job runs. This job is configured to run on all server roles.
    /// </summary>
    /// <remarks>Runs on all servers</remarks>
    public override ServerRole[] ServerRoles => Enum.GetValues<ServerRole>();

    private readonly TimeSpan _age = TimeSpan.FromDays(1);
    private readonly IIOHelper _ioHelper;
    private readonly ILogger<TempFileCleanupJob> _logger;
    private readonly DirectoryInfo[] _tempFolders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TempFileCleanupJob" /> class.
    /// </summary>
    /// <param name="ioHelper">Helper service for IO operations.</param>
    /// <param name="logger">The typed logger.</param>
    public TempFileCleanupJob(IIOHelper ioHelper, ILogger<TempFileCleanupJob> logger)
    {
        _ioHelper = ioHelper;
        _logger = logger;

        _tempFolders = _ioHelper.GetTempFolders();
    }

    /// <summary>
    /// Asynchronously executes the cleanup of temporary files in the configured temporary folders.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that is signaled when the host is shutting down.</param>
    /// <returns>
    /// A task that represents the asynchronous cleanup operation.
    /// </returns>
    public override Task RunJobAsync(CancellationToken cancellationToken)
    {
        foreach (DirectoryInfo folder in _tempFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CleanupFolder(folder, cancellationToken);
        }

        return Task.CompletedTask;
    }

    private void CleanupFolder(DirectoryInfo folder, CancellationToken cancellationToken)
    {
        CleanFolderResult result = _ioHelper.CleanFolder(folder, _age);
        switch (result.Status)
        {
            case CleanFolderResultStatus.FailedAsDoesNotExist:
                _logger.LogDebug("The cleanup folder doesn't exist {Folder}", folder.FullName);
                break;
            case CleanFolderResultStatus.FailedWithException:
                foreach (CleanFolderResult.Error error in result.Errors!)
                {
                    _logger.LogError(
                        error.Exception,
                        "Could not delete temp file {FileName}",
                        error.ErroringFile.FullName);
                }

                break;
        }

        folder.Refresh(); // In case it's changed during runtime
        if (!folder.Exists)
        {
            _logger.LogDebug("The cleanup folder doesn't exist {Folder}", folder.FullName);
            return;
        }

        FileInfo[] files = folder.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (FileInfo file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow - file.LastWriteTimeUtc > _age)
            {
                try
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not delete temp file {FileName}", file.FullName);
                }
            }
        }
    }
}
