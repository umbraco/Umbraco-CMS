using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;

namespace Umbraco.Cms.Infrastructure.Runtime
{
    internal class FileSystemMainDomLock : IMainDomLock
    {
        private readonly ILogger<FileSystemMainDomLock> _logger;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly string _lockFilePath;
        private readonly string _releaseSignalFilePath;

        private FileStream? _lockFileStream;
        private Task? _listenForReleaseSignalFileTask;

        public FileSystemMainDomLock(
            ILogger<FileSystemMainDomLock> logger,
            IMainDomKeyGenerator mainDomKeyGenerator,
            IHostingEnvironment hostingEnvironment,
            IOptionsMonitor<GlobalSettings> globalSettings)
        {
            _logger = logger;
            _globalSettings = globalSettings;

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
                    DeleteLockReleaseSignalFile();
                    return Task.FromResult(true);
                }
                catch (IOException)
                {
                    _logger.LogDebug("Couldn't obtain MainDom lock file handle, signalling for release of {lockFilePath}", _lockFilePath);
                    CreateLockReleaseSignalFile();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected exception attempting to obtain MainDom lock file handle {lockFilePath}, giving up", _lockFilePath);
                    _lockFileStream?.Close();
                    return Task.FromResult(false);
                }
            }
            while (stopwatch.ElapsedMilliseconds < millisecondsTimeout);

            return Task.FromResult(false);
        }

        public void CreateLockReleaseSignalFile() =>
            File.Open(_releaseSignalFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete)
                .Close();

        public void DeleteLockReleaseSignalFile() =>
            File.Delete(_releaseSignalFilePath);

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

                Thread.Sleep(_globalSettings.CurrentValue.MainDomReleaseSignalPollingInterval);
            }
        }

        public void Dispose()
        {
            _lockFileStream?.Close();
            _lockFileStream = null;
        }
    }
}
