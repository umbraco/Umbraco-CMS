using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
    public abstract class AbstractFileSystem : IFileSystem
    {
        public abstract IEnumerable<string> GetDirectories(string path);

        public abstract void DeleteDirectory(string path);

        public abstract void DeleteDirectory(string path, bool recursive);

        public abstract bool DirectoryExists(string path);

        public abstract void AddFile(string path, Stream stream);

        public abstract void AddFile(string path, Stream stream, bool overrideIfExists);

        public abstract IEnumerable<string> GetFiles(string path);

        public abstract IEnumerable<string> GetFiles(string path, string filter);

        public abstract Stream OpenFile(string path);

        public abstract void DeleteFile(string path);

        public abstract bool FileExists(string path);

        public abstract string GetRelativePath(string fullPathOrUrl);

        public abstract string GetFullPath(string path);

        public abstract string GetUrl(string path);

        public virtual long GetSize(string path)
        {
            var s = OpenFile(path);
            var size = s.Length;
            s.Close();

            return size;
        }

        public abstract DateTimeOffset GetLastModified(string path);

        public abstract DateTimeOffset GetCreated(string path);
    }
}
