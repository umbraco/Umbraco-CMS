// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Used to cleanup temporary file locations.
/// </summary>
/// <remarks>
///     Will run on all servers - even though file upload should only be handled on the scheduling publisher, this will
///     ensure that in the case it happens on subscribers that they are cleaned up too.
/// </remarks>
public class TempFileCleanupJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromMinutes(60); }

    // Runs on all servers
    public ServerRole[] ServerRoles { get => Enum.GetValues<ServerRole>(); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

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

    public Task RunJobAsync()
    {
        foreach (DirectoryInfo folder in _tempFolders)
        {
            CleanupFolder(folder);
        }

        return Task.CompletedTask;
    }

    private void CleanupFolder(DirectoryInfo folder)
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
                    _logger.LogError(error.Exception, "Could not delete temp file {FileName}",
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
