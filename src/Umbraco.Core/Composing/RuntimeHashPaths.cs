using System.Collections.Generic;
using System.IO;

namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Paths used to determine the <see cref="IRuntimeHash"/>
    /// </summary>
    public sealed class RuntimeHashPaths
    {
        private readonly List<DirectoryInfo> _paths = new List<DirectoryInfo>();
        private readonly Dictionary<FileInfo, bool> _files = new Dictionary<FileInfo, bool>();

        public RuntimeHashPaths AddFolder(DirectoryInfo pathInfo)
        {
            _paths.Add(pathInfo);
            return this;
        }

        public void AddFile(FileInfo fileInfo, bool scanFileContent = false) => _files.Add(fileInfo, scanFileContent);

        public IEnumerable<DirectoryInfo> GetFolders() => _paths;
        public IReadOnlyDictionary<FileInfo, bool> GetFiles() => _files;
    }
}
