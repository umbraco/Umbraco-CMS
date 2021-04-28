using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Scheduling
{
    /// <summary>
    /// Used to cleanup temporary file locations
    /// </summary>
    internal class TempFileCleanup : RecurringTaskBase
    {
        private readonly DirectoryInfo[] _tempFolders;
        private readonly TimeSpan _age;
        private readonly IRuntimeState _runtime;
        private readonly IProfilingLogger _logger;

        public TempFileCleanup(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IEnumerable<DirectoryInfo> tempFolders, TimeSpan age,
            IRuntimeState runtime, IProfilingLogger logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            //SystemDirectories.TempFileUploads

            _tempFolders = tempFolders.ToArray();
            _age = age;
            _runtime = runtime;
            _logger = logger;
        }

        public override bool PerformRun()
        {
            // ensure we do not run if not main domain
            if (_runtime.IsMainDom == false)
            {
                _logger.Debug<TempFileCleanup>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            foreach (var dir in _tempFolders)
                CleanupFolder(dir);

            return true; //repeat
        }

        private void CleanupFolder(DirectoryInfo dir)
        {
            dir.Refresh(); //in case it's changed during runtime
            if (!dir.Exists)
            {
                _logger.Debug<TempFileCleanup, string>("The cleanup folder doesn't exist {Folder}", dir.FullName);
                return;
            }

            var files = dir.GetFiles("*.*", SearchOption.AllDirectories);
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
                        _logger.Error<TempFileCleanup, string>(ex, "Could not delete temp file {FileName}", file.FullName);
                    }
                }
            }
        }

        public override bool IsAsync => false;
    }
}
