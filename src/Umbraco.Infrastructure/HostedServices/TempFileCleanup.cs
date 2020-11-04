using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Used to cleanup temporary file locations.
    /// </summary>
    /// <remarks>
    /// Will run on all servers - even though file upload should only be handled on the master, this will
    /// ensure that in the case it happes on replicas that they are cleaned up too.
    /// </remarks>
    [UmbracoVolatile]
    public class TempFileCleanup : RecurringHostedServiceBase
    {
        private readonly IIOHelper _ioHelper;
        private readonly IMainDom _mainDom;
        private readonly ILogger<TempFileCleanup> _logger;

        private readonly DirectoryInfo[] _tempFolders;
        private readonly TimeSpan _age = TimeSpan.FromDays(1);

        public TempFileCleanup(IIOHelper ioHelper, IMainDom mainDom, ILogger<TempFileCleanup> logger)
            : base(TimeSpan.FromMinutes(60), DefaultDelay)
        {
            _ioHelper = ioHelper;
            _mainDom = mainDom;
            _logger = logger;

            _tempFolders = _ioHelper.GetTempFolders();
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            // Ensure we do not run if not main domain
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return;
            }

            foreach (var folder in _tempFolders)
            {
                CleanupFolder(folder);
            }

            return;
        }

        private void CleanupFolder(DirectoryInfo folder)
        {
            var result = _ioHelper.CleanFolder(folder, _age);
            switch (result.Status)
            {
                case CleanFolderResultStatus.FailedAsDoesNotExist:
                    _logger.LogDebug("The cleanup folder doesn't exist {Folder}", folder.FullName);
                    break;
                case CleanFolderResultStatus.FailedWithException:
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError(error.Exception, "Could not delete temp file {FileName}", error.ErroringFile.FullName);
                    }

                    break;
            }

            folder.Refresh(); // In case it's changed during runtime
            if (!folder.Exists)
            {
                _logger.LogDebug("The cleanup folder doesn't exist {Folder}", folder.FullName);
                return;
            }

            var files = folder.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var file in files)
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
}
