using System.Collections.Concurrent;
using System.IO;
using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    internal class LocalTempStorageDirectoryTracker
    {
        private readonly static LocalTempStorageDirectoryTracker Instance = new LocalTempStorageDirectoryTracker();
        private readonly ConcurrentDictionary<string, LocalTempStorageDirectory> _directories = new ConcurrentDictionary<string, LocalTempStorageDirectory>();

        public static LocalTempStorageDirectoryTracker Current
        {
            get { return Instance; }
        }

        public LocalTempStorageDirectory GetDirectory(DirectoryInfo dir, FSDirectory realDir, bool disable = false)
        {
            var resolved = _directories.GetOrAdd(dir.FullName, s => new LocalTempStorageDirectory(dir, realDir));

            if (disable)
            {
                resolved.Enabled = false;
            }
            
            return resolved;
        }
    }
}