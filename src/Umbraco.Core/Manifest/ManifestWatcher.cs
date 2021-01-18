using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Hosting;

namespace Umbraco.Core.Manifest
{
    public class ManifestWatcher : IDisposable
    {
        private static readonly object Locker = new object();
        private static volatile bool _isRestarting;

        private readonly ILogger<ManifestWatcher> _logger;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly List<FileSystemWatcher> _fws = new List<FileSystemWatcher>();
        private bool _disposed;

        public ManifestWatcher(ILogger<ManifestWatcher> logger, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
        }

        public void Start(params string[] packageFolders)
        {
            foreach (var packageFolder in packageFolders.Where(IsWatchable))
            {
                // for some reason *.manifest doesn't work
                var fsw = new FileSystemWatcher(packageFolder, "*package.*")
                {
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                };

                _fws.Add(fsw);

                fsw.Changed += FswChanged;
                fsw.EnableRaisingEvents = true;
            }
        }

        private static bool IsWatchable(string folder)
        {
            return Directory.Exists(folder) && File.Exists(Path.Combine(folder, "package.manifest"));
        }

        private void FswChanged(object sender, FileSystemEventArgs e)
        {
            if (!e.Name.InvariantContains("package.manifest"))
            {
                return;
            }

            // ensure the app is not restarted multiple times for multiple
            // savings during the same app domain execution - restart once
            lock (Locker)
            {
                if (_isRestarting) return;

                _isRestarting = true;
                _logger.LogInformation("Manifest has changed, app pool is restarting ({Path})", e.FullPath);
                _umbracoApplicationLifetime.Restart();
            }
        }

        private void Dispose(bool disposing)
        {
            // ReSharper disable InvertIf
            if (disposing && !_disposed)
            {
                foreach (FileSystemWatcher fw in _fws)
                {
                    fw.Dispose();
                }

                _disposed = true;
            }

            // ReSharper restore InvertIf
        }

        public void Dispose() => Dispose(true);
    }
}
