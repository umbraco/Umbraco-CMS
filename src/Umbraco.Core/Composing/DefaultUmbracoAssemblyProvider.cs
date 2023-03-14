using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Returns a list of scannable assemblies based on an entry point assembly and it's references
/// </summary>
/// <remarks>
///     This will recursively search through the entry point's assemblies and Umbraco's core assemblies and their
///     references
///     to create a list of scannable assemblies based on whether they themselves or their transitive dependencies
///     reference Umbraco core assemblies.
/// </remarks>
public class DefaultUmbracoAssemblyProvider : IAssemblyProvider
{
    private readonly IEnumerable<string>? _additionalTargetAssemblies;
    private readonly Assembly _entryPointAssembly;
    private readonly ILoggerFactory _loggerFactory;
    private List<Assembly>? _discovered;

    public DefaultUmbracoAssemblyProvider(
        Assembly? entryPointAssembly,
        ILoggerFactory loggerFactory,
        IEnumerable<string>? additionalTargetAssemblies = null)
    {
        _entryPointAssembly = entryPointAssembly ?? throw new ArgumentNullException(nameof(entryPointAssembly));
        _loggerFactory = loggerFactory;
        _additionalTargetAssemblies = additionalTargetAssemblies;
    }

    // TODO: It would be worth investigating a netcore3 version of this which would use
    // var allAssemblies = System.Runtime.Loader.AssemblyLoadContext.All.SelectMany(x => x.Assemblies);
    // that will still only resolve Assemblies that are already loaded but it would also make it possible to
    // query dynamically generated assemblies once they are added. It would also provide the ability to probe
    // assembly locations that are not in the same place as the entry point assemblies.
    public IEnumerable<Assembly> Assemblies
    {
        get
        {
            if (_discovered != null)
            {
                return _discovered;
            }

            IEnumerable<string> additionalTargetAssemblies = Constants.Composing.UmbracoCoreAssemblyNames;
            if (_additionalTargetAssemblies != null)
            {
                additionalTargetAssemblies = additionalTargetAssemblies.Concat(_additionalTargetAssemblies);
            }

            var finder = new FindAssembliesWithReferencesTo(
                new[] { _entryPointAssembly },
                additionalTargetAssemblies.ToArray(),
                true,
                _loggerFactory);
            _discovered = finder.Find().ToList();

            return _discovered;
        }
    }
}
