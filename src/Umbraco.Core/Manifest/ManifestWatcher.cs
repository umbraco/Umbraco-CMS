using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Manifest
{
    internal class ManifestWatcher : DisposableObject
    {
        private readonly ILogger _logger;
        private readonly List<FileSystemWatcher> _fws = new List<FileSystemWatcher>();
        private static volatile bool _isRestarting = false;
        private static readonly object Locker = new object();

        public ManifestWatcher(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public void Start(params string[] packageFolders)
        {
            foreach (var packageFolder in packageFolders)
            {
                if (Directory.Exists(packageFolder) && File.Exists(Path.Combine(packageFolder, "package.manifest")))
                {
                    //NOTE: for some reason *.manifest doesn't work!
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
        }

        void FswChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name.InvariantContains("package.manifest"))
            {
                //Ensure the app is not restarted multiple times for multiple saving during the same app domain execution
                if (_isRestarting == false)
                {
                    lock (Locker)
                    {
                        if (_isRestarting == false)
                        {
                            _isRestarting = true;

                            _logger.Info<ManifestWatcher>("manifest has changed, app pool is restarting (" + e.FullPath + ")");
                            HttpRuntime.UnloadAppDomain();
                            Dispose();               
                        }
                    }
                }                
            }
        }

        protected override void DisposeResources()
        {
            foreach (var fw in _fws)
            {
                fw.Dispose();
            }
        }
    }
}