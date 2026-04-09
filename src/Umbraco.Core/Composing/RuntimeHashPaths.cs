using System.Reflection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Paths used to determine the <see cref="IRuntimeHash" />
/// </summary>
public sealed class RuntimeHashPaths
{
    private readonly Dictionary<FileInfo, bool> _files = new();
    private readonly List<DirectoryInfo> _paths = new();

    /// <summary>
    /// Adds a folder to be included in the runtime hash calculation.
    /// </summary>
    /// <param name="pathInfo">The directory information for the folder to add.</param>
    /// <returns>The current <see cref="RuntimeHashPaths" /> instance for method chaining.</returns>
    public RuntimeHashPaths AddFolder(DirectoryInfo pathInfo)
    {
        _paths.Add(pathInfo);
        return this;
    }

    /// <summary>
    ///     Creates a runtime hash based on the assembly provider
    /// </summary>
    /// <param name="assemblyProvider"></param>
    /// <returns></returns>
    public RuntimeHashPaths AddAssemblies(IAssemblyProvider assemblyProvider)
    {
        foreach (Assembly assembly in assemblyProvider.Assemblies)
        {
            // TODO: We need to test this on a published website
            if (!assembly.IsDynamic && assembly.Location != null)
            {
                AddFile(new FileInfo(assembly.Location));
            }
        }

        return this;
    }

    /// <summary>
    /// Adds a file to be included in the runtime hash calculation.
    /// </summary>
    /// <param name="fileInfo">The file information for the file to add.</param>
    /// <param name="scanFileContent">If set to <c>true</c>, the file content is scanned; otherwise, only file metadata is used.</param>
    public void AddFile(FileInfo fileInfo, bool scanFileContent = false) => _files.Add(fileInfo, scanFileContent);

    /// <summary>
    /// Gets all folders that have been added for hash calculation.
    /// </summary>
    /// <returns>A collection of directory information objects.</returns>
    public IEnumerable<DirectoryInfo> GetFolders() => _paths;

    /// <summary>
    /// Gets all files that have been added for hash calculation.
    /// </summary>
    /// <returns>A dictionary mapping file information to a boolean indicating whether to scan content.</returns>
    public IReadOnlyDictionary<FileInfo, bool> GetFiles() => _files;
}
