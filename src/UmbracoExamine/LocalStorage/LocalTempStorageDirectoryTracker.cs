using System.Collections.Concurrent;
using System.IO;
using Directory = Lucene.Net.Store.Directory;

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

        public LocalTempStorageDirectory GetDirectory(DirectoryInfo dir, Directory realDir, bool disable = false)
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