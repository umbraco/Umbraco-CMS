// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Used to cleanup temporary file locations.
/// </summary>
/// <remarks>
///     Will run on all servers - even though file upload should only be handled on the scheduling publisher, this will
///     ensure that in the case it happens on subscribers that they are cleaned up too.
/// </remarks>
public class TempFileCleanup : RecurringHostedServiceBase
{
    private readonly TimeSpan _age = TimeSpan.FromDays(1);
    private readonly IIOHelper _ioHelper;
    private readonly ILogger<TempFileCleanup> _logger;
    private readonly IMainDom _mainDom;

    private readonly DirectoryInfo[] _tempFolders;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TempFileCleanup" /> class.
    /// </summary>
    /// <param name="ioHelper">Helper service for IO operations.</param>
    /// <param name="mainDom">Representation of the main application domain.</param>
    /// <param name="logger">The typed logger.</param>
    public TempFileCleanup(IIOHelper ioHelper, IMainDom mainDom, ILogger<TempFileCleanup> logger)
        : base(logger, TimeSpan.FromMinutes(60), DefaultDelay)
    {
        _ioHelper = ioHelper;
        _mainDom = mainDom;
        _logger = logger;

        _tempFolders = _ioHelper.GetTempFolders();
    }

    public override Task PerformExecuteAsync(object? state)
    {
        // Ensure we do not run if not main domain
        if (_mainDom.IsMainDom == false)
        {
            _logger.LogDebug("Does not run if not MainDom.");
            return Task.CompletedTask;
        }

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
