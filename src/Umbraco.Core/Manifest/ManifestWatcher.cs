using System.Collections.Generic;
using System.IO;
using System.Web;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Manifest
{
    internal class ManifestWatcher : DisposableObject
    {
        private readonly List<FileSystemWatcher> _fws = new List<FileSystemWatcher>();

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
                LogHelper.Info<ManifestWatcher>("manifest has changed, app pool is restarting (" + e.FullPath + ")");
                HttpRuntime.UnloadAppDomain();
                Dispose();    
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