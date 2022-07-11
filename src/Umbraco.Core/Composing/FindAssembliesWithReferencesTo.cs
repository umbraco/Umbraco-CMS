using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Finds Assemblies from the entry point assemblies, it's dependencies and it's transitive dependencies that reference
///     that targetAssemblyNames
/// </summary>
/// <remarkes>
///     borrowed and modified from here
///     https://github.com/dotnet/aspnetcore-tooling/blob/master/src/Razor/src/Microsoft.NET.Sdk.Razor/FindAssembliesWithReferencesTo.cs
/// </remarkes>
internal class FindAssembliesWithReferencesTo
{
    private readonly bool _includeTargets;
    private readonly ILogger<FindAssembliesWithReferencesTo> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Assembly[] _referenceAssemblies;
    private readonly string[] _targetAssemblies;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="referenceAssemblies">Entry point assemblies</param>
    /// <param name="targetAssemblyNames">
    ///     Used to check if the entry point or it's transitive assemblies reference these
    ///     assembly names
    /// </param>
    /// <param name="includeTargets">If true will also use the target assembly names as entry point assemblies</param>
    /// <param name="loggerFactory">Logger factory for when scanning goes wrong</param>
    public FindAssembliesWithReferencesTo(Assembly[] referenceAssemblies, string[] targetAssemblyNames, bool includeTargets, ILoggerFactory loggerFactory)
    {
        _referenceAssemblies = referenceAssemblies;
        _targetAssemblies = targetAssemblyNames;
        _includeTargets = includeTargets;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<FindAssembliesWithReferencesTo>();
    }

    public IEnumerable<Assembly> Find()
    {
        var referenceItems = new List<Assembly>();
        foreach (Assembly assembly in _referenceAssemblies)
        {
            referenceItems.Add(assembly);
        }

        if (_includeTargets)
        {
            foreach (var target in _targetAssemblies)
            {
                try
                {
                    referenceItems.Add(Assembly.Load(target));
                }
                catch (FileNotFoundException ex)
                {
                    // occurs if we cannot load this ... for example in a test project where we aren't currently referencing Umbraco.Web, etc...
                    _logger.LogDebug(ex, "Could not load assembly " + target);
                }
            }
        }

        var provider = new ReferenceResolver(_targetAssemblies, referenceItems, _loggerFactory.CreateLogger<ReferenceResolver>());
        IEnumerable<Assembly> assemblyNames = provider.ResolveAssemblies();
        return assemblyNames.ToList();
    }
}
