using System;
using System.Collections.Concurrent;
using System.IO;
using Lucene.Net.Store;

namespace UmbracoExamine.LocalStorage
{
    [Obsolete("This is used only for configuration based indexes")]
    internal class DirectoryTracker
    {
        private static readonly DirectoryTracker Instance = new DirectoryTracker();

        private readonly ConcurrentDictionary<string, Lucene.Net.Store.Directory> _directories = new ConcurrentDictionary<string, Lucene.Net.Store.Directory>();

        public static DirectoryTracker Current
        {
            get { return Instance; }
        }

        public Lucene.Net.Store.Directory GetDirectory(DirectoryInfo dir)
        {
            var resolved = _directories.GetOrAdd(dir.FullName, s => new SimpleFSDirectory(dir));
            return resolved;
        }
    }
}