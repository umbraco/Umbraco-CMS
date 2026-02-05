using System.Reflection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Provides a list of assemblies that can be scanned.
/// </summary>
public interface IAssemblyProvider
{
    /// <summary>
    /// Gets the collection of assemblies available for type scanning.
    /// </summary>
    IEnumerable<Assembly> Assemblies { get; }
}
