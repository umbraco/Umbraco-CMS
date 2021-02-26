using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Core.Composing
{
    /// <summary>
    /// Determines the runtime hash based on file system paths to scan
    /// </summary>
    public class RuntimeHash : IRuntimeHash
    {
        private readonly IProfilingLogger _logger;
        private readonly RuntimeHashPaths _paths;

        public RuntimeHash(IProfilingLogger logger, RuntimeHashPaths paths)
        {
            _logger = logger;
            _paths = paths;
        }


        public string GetHashValue()
        {
            var allPaths = _paths.GetFolders()
                .Select(x => ((FileSystemInfo) x, false))
                .Concat(_paths.GetFiles().Select(x => ((FileSystemInfo) x.Key, x.Value)));

            var hash = GetFileHash(allPaths);

            return hash;
        }

        /// <summary>
        /// Returns a unique hash for a combination of FileInfo objects.
        /// </summary>
        /// <param name="filesAndFolders">A collection of files.</param>
        /// <returns>The hash.</returns>
        /// <remarks>Each file is a tuple containing the FileInfo object and a boolean which indicates whether to hash the
        /// file properties (false) or the file contents (true).</remarks>
        private string GetFileHash(IEnumerable<(FileSystemInfo fileOrFolder, bool scanFileContent)> filesAndFolders)
        {
            using (_logger.DebugDuration<TypeLoader>("Determining hash of code files on disk", "Hash determined"))
            {
                // get the distinct file infos to hash
                var uniqInfos = new HashSet<string>();
                var uniqContent = new HashSet<string>();

                using var generator = new HashGenerator();

                foreach (var (fileOrFolder, scanFileContent) in filesAndFolders)
                {
                    if (scanFileContent)
                    {
                        // add each unique file's contents to the hash
                        // normalize the content for cr/lf and case-sensitivity
                        if (uniqContent.Add(fileOrFolder.FullName))
                        {
                            if (File.Exists(fileOrFolder.FullName) == false) continue;
                            var content = RemoveCrLf(File.ReadAllText(fileOrFolder.FullName));
                            generator.AddCaseInsensitiveString(content);
                        }
                    }
                    else
                    {
                        // add each unique folder/file to the hash
                        if (uniqInfos.Add(fileOrFolder.FullName))
                        {
                            generator.AddFileSystemItem(fileOrFolder);
                        }
                    }
                }
                return generator.GenerateHash();
            }
        }

        // fast! (yes, according to benchmarks)
        private static string RemoveCrLf(string s)
        {
            var buffer = new char[s.Length];
            var count = 0;
            // ReSharper disable once ForCanBeConvertedToForeach - no!
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] != '\r' && s[i] != '\n')
                    buffer[count++] = s[i];
            }
            return new string(buffer, 0, count);
        }
    }
}
