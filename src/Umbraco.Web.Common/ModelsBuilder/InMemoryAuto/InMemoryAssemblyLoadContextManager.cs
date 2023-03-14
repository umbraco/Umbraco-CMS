using System.Reflection;
using System.Runtime.Loader;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Web.Common.ModelsBuilder.InMemoryAuto;

internal class InMemoryAssemblyLoadContextManager
{
    private UmbracoAssemblyLoadContext? _currentAssemblyLoadContext;

    public InMemoryAssemblyLoadContextManager() =>
        AssemblyLoadContext.Default.Resolving += OnResolvingDefaultAssemblyLoadContext;

    private string? _modelsAssemblyLocation;

    public string? ModelsAssemblyLocation => _modelsAssemblyLocation;

    /// <summary>
    /// Handle the event when a reference cannot be resolved from the default context and return our custom MB assembly reference if we have one
    /// </summary>
    /// <remarks>
    /// This is required because the razor engine will only try to load things from the default context, it doesn't know anything
    /// about our context so we need to proxy.
    /// </remarks>
    private Assembly? OnResolvingDefaultAssemblyLoadContext(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
        => assemblyName.Name == RoslynCompiler.GeneratedAssemblyName
            ? _currentAssemblyLoadContext?.LoadFromAssemblyName(assemblyName)
            : null;

    internal void RenewAssemblyLoadContext()
    {
        // If there's a current AssemblyLoadContext, unload it before creating a new one.
        _currentAssemblyLoadContext?.Unload();

        // We must create a new assembly load context
        // as long as theres a reference to the assembly load context we can't delete the assembly it loaded
        _currentAssemblyLoadContext = new UmbracoAssemblyLoadContext();
    }

    /// <summary>
    /// Loads an assembly into the collectible assembly used by the factory
    /// </summary>
    /// <remarks>
    /// This is essentially just a wrapper around the <see cref="UmbracoAssemblyLoadContext"/>,
    /// because we don't want to allow other clases to take a reference on the AssemblyLoadContext
    /// </remarks>
    /// <returns>The loaded assembly</returns>
    public Assembly LoadCollectibleAssemblyFromStream(Stream assembly, Stream? assemblySymbols)
    {
        _currentAssemblyLoadContext ??= new UmbracoAssemblyLoadContext();
        return _currentAssemblyLoadContext.LoadFromStream(assembly, assemblySymbols);
    }

    public Assembly LoadCollectibleAssemblyFromPath(string path)
    {
        _currentAssemblyLoadContext ??= new UmbracoAssemblyLoadContext();
        return _currentAssemblyLoadContext.LoadFromAssemblyPath(path);
    }

    public Assembly LoadModelsAssembly(string path)
    {
        Assembly assembly = LoadCollectibleAssemblyFromPath(path);
        _modelsAssemblyLocation = assembly.Location;
        return assembly;
    }
}
