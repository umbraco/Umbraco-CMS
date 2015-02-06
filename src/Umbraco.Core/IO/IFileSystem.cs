using System;
using System.Collections.Generic;
using System.IO;

namespace Umbraco.Core.IO
{
    //TODO: There is no way to create a directory here without creating a file in a directory and then deleting it
    //TODO: Should probably implement a rename?

	public interface IFileSystem
    {
        IEnumerable<string> GetDirectories(string path);

        void DeleteDirectory(string path);

        void DeleteDirectory(string path, bool recursive);

        bool DirectoryExists(string path);
        
        void AddFile(string path, Stream stream);

        void AddFile(string path, Stream stream, bool overrideIfExists);

        IEnumerable<string> GetFiles(string path);

        IEnumerable<string> GetFiles(string path, string filter);

        Stream OpenFile(string path);

        void DeleteFile(string path);

        bool FileExists(string path);


        string GetRelativePath(string fullPathOrUrl);

        string GetFullPath(string path);

        string GetUrl(string path);

        DateTimeOffset GetLastModified(string path);

        DateTimeOffset GetCreated(string path);
    }
}
