using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Sync
{
    public sealed class LastSyncedFileManager
    {
        private string _distCacheFile;
        private bool _lastIdReady;
        private object _lastIdLock;
        private int _lastId;
        private readonly IHostingEnvironment _hostingEnvironment;

        public LastSyncedFileManager(IHostingEnvironment hostingEnvironment)
            => _hostingEnvironment = hostingEnvironment;

        /// <summary>
        /// Persists the last-synced id to file.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Thread safety: this is NOT thread safe. Because it is NOT meant to run multi-threaded.
        /// </remarks>
        public void SaveLastSyncedId(int id)
        {
            File.WriteAllText(DistCacheFilePath, id.ToString(CultureInfo.InvariantCulture));
            _lastId = id;
        }

        /// <summary>
        /// Reads the last-synced id from file (once).
        /// </summary>
        public int LastSyncedId => LazyInitializer.EnsureInitialized(
                ref _lastId,
                ref _lastIdReady,
                ref _lastIdLock,
                () =>
                {
                    var distCacheFilePath = DistCacheFilePath;

                    if (File.Exists(distCacheFilePath))
                    {
                        var content = File.ReadAllText(distCacheFilePath);
                        if (int.TryParse(content, out var last))
                        {
                            return last;
                        }
                    }

                    return -1;
                });

        /// <summary>
        /// Gets the dist cache file path (once).
        /// </summary>
        /// <returns></returns>
        public string DistCacheFilePath => LazyInitializer.EnsureInitialized(ref _distCacheFile, () =>
         {
             var fileName = (Environment.MachineName + _hostingEnvironment.ApplicationId).GenerateHash() + "-lastsynced.txt";

             var distCacheFilePath = Path.Combine(_hostingEnvironment.LocalTempPath, "DistCache", fileName);

             //ensure the folder exists
             var folder = Path.GetDirectoryName(distCacheFilePath);
             if (folder == null)
             {
                 throw new InvalidOperationException("The folder could not be determined for the file " + distCacheFilePath);
             }

             if (Directory.Exists(folder) == false)
             {
                 Directory.CreateDirectory(folder);
             }

             return distCacheFilePath;
         });
    }
}
