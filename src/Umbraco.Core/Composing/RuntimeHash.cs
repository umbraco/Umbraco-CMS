using Umbraco.Cms.Core.Logging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Determines the runtime hash based on file system paths to scan
/// </summary>
public class RuntimeHash : IRuntimeHash
{
    private readonly IProfilingLogger _logger;
    private readonly RuntimeHashPaths _paths;
    private string? _calculated;

    public RuntimeHash(IProfilingLogger logger, RuntimeHashPaths paths)
    {
        _logger = logger;
        _paths = paths;
    }

    public string GetHashValue()
    {
        if (_calculated != null)
        {
            return _calculated;
        }

        IEnumerable<(FileSystemInfo, bool)> allPaths = _paths.GetFolders()
            .Select(x => ((FileSystemInfo)x, false))
            .Concat(_paths.GetFiles().Select(x => ((FileSystemInfo)x.Key, x.Value)));

        _calculated = GetFileHash(allPaths);

        return _calculated;
    }

    /// <summary>
    ///     Returns a unique hash for a combination of FileInfo objects.
    /// </summary>
    /// <param name="filesAndFolders">A collection of files.</param>
    /// <returns>The hash.</returns>
    /// <remarks>
    ///     Each file is a tuple containing the FileInfo object and a boolean which indicates whether to hash the
    ///     file properties (false) or the file contents (true).
    /// </remarks>
    private string GetFileHash(IEnumerable<(FileSystemInfo fileOrFolder, bool scanFileContent)> filesAndFolders)
    {
        using (_logger.DebugDuration<RuntimeHash>("Determining hash of code files on disk", "Hash determined"))
        {
            // get the distinct file infos to hash
            var uniqInfos = new HashSet<string>();
            var uniqContent = new HashSet<string>();

            using var generator = new HashGenerator();

            foreach ((FileSystemInfo fileOrFolder, var scanFileContent) in filesAndFolders)
            {
                if (scanFileContent)
                {
                    // add each unique file's contents to the hash
                    // normalize the content for cr/lf and case-sensitivity
                    if (uniqContent.Add(fileOrFolder.FullName))
                    {
                        if (File.Exists(fileOrFolder.FullName) == false)
                        {
                            continue;
                        }

                        using (FileStream fileStream = File.OpenRead(fileOrFolder.FullName))
                        {
                            var hash = fileStream.GetStreamHash();
                            generator.AddCaseInsensitiveString(hash);
                        }
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
}
