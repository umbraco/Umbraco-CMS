using System.Reflection;
using System.Runtime.Loader;

/// <inheritdoc />
internal class PluginLoadContext : AssemblyLoadContext
{
    /// <summary>
    /// The resolver.
    /// </summary>
    private readonly AssemblyDependencyResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoadContext" /> class.
    /// </summary>
    /// <param name="pluginPath">The plugin path.</param>
    public PluginLoadContext(string pluginPath)
        => _resolver = new AssemblyDependencyResolver(pluginPath);

    /// <inheritdoc />
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    /// <inheritdoc />
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}
