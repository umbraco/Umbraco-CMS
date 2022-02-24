using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Infrastructure.Runtime
{
    internal class FileSystemMainDomLock : IMainDomLock
    {
        private readonly ILogger<FileSystemMainDomLock> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly string _lockFilePath;
        private readonly string _releaseSignalFilePath;

        private FileStream _lockFileStream;
        private Task _listenForReleaseSignalFileTask;

        private const int s_maxTriesRemovingLockReleaseSignalFile = 3;

        public FileSystemMainDomLock(
            ILogger<FileSystemMainDomLock> logger,
            IMainDomKeyGenerator mainDomKeyGenerator,
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;

            var lockFileName = $"MainDom_{mainDomKeyGenerator.GenerateKey()}.lock";
            _lockFilePath = Path.Combine(hostingEnvironment.LocalTempPath, lockFileName);
            _releaseSignalFilePath = $"{_lockFilePath}_release";
        }

        public Task<bool> AcquireLockAsync(int millisecondsTimeout)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                try
                {
                    _logger.LogDebug("Attempting to obtain MainDom lock file handle {lockFilePath}", _lockFilePath);
                    _lockFileStream = File.Open(_lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    DeleteLockReleaseFile();
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    _logger.LogDebug("Couldn't obtain MainDom lock file handle, signalling for release of {lockFilePath}", _lockFilePath);
                    CreateLockReleaseFile();
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected exception attempting to obtain MainDom lock file handle {lockFilePath}, giving up", _lockFilePath);
                    return Task.FromResult(false);
                }
            }
            while (stopwatch.ElapsedMilliseconds < millisecondsTimeout);

            return Task.FromResult(false);
        }

        // Create a long running task to poll to check if anyone has created a lock release file.
        public Task ListenAsync()
        {
            if (_listenForReleaseSignalFileTask != null)
            {
                return _listenForReleaseSignalFileTask;
            }

            _listenForReleaseSignalFileTask = Task.Factory.StartNew(
                ListeningLoop,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return _listenForReleaseSignalFileTask;
        }

        public void Dispose()
        {
            _lockFileStream?.Close();
            _lockFileStream = null;
        }

        private void CreateLockReleaseFile()
        {
            try
            {
                // Dispose immediately to release the file handle so it's easier to cleanup in any process.
                using FileStream releaseFileStream = File.Open(_releaseSignalFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception attempting to create lock release signal file {file}", _releaseSignalFilePath);
            }
        }

        private void DeleteLockReleaseFile()
        {
            List<Exception> encounteredExceptions = new();
            for (var i = 0; i < s_maxTriesRemovingLockReleaseSignalFile; i++)
            {
                try
                {
                    File.Delete(_releaseSignalFilePath);
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected exception attempting to delete release signal file {file}", _releaseSignalFilePath);
                    encounteredExceptions.Add(ex);
                    Thread.Sleep(500 * (i + 1));
                }
            }

            throw new ApplicationException($"Failed to remove lock release signal file {_releaseSignalFilePath}", new AggregateException(encounteredExceptions));
        }

        private void ListeningLoop()
        {
            while (true)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _logger.LogDebug("ListenAsync Task canceled, exiting loop");
                    return;
                }

                if (File.Exists(_releaseSignalFilePath))
                {
                    _logger.LogDebug("Found lock release signal file, releasing lock on {lockFilePath}", _lockFilePath);
                    _lockFileStream?.Close();
                    _lockFileStream = null;
                    break;
                }

                Thread.Sleep(2000);
            }
        }
    }
}
