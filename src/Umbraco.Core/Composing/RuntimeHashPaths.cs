using System.Reflection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Paths used to determine the <see cref="IRuntimeHash" />
/// </summary>
public sealed class RuntimeHashPaths
{
    private readonly Dictionary<FileInfo, bool> _files = new();
    private readonly List<DirectoryInfo> _paths = new();

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

    public void AddFile(FileInfo fileInfo, bool scanFileContent = false) => _files.Add(fileInfo, scanFileContent);

    public IEnumerable<DirectoryInfo> GetFolders() => _paths;

    public IReadOnlyDictionary<FileInfo, bool> GetFiles() => _files;
}
