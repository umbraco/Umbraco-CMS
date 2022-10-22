using System.Reflection;
using System.Runtime.Loader;

namespace Umbraco.Cms.Web.Common.ModelsBuilder;

internal class UmbracoAssemblyLoadContext : AssemblyLoadContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoAssemblyLoadContext" /> class.
    /// </summary>
    /// <remarks>
    ///     Collectible AssemblyLoadContext used to load in the compiled generated models.
    ///     Must be a collectible assembly in order to be able to be unloaded.
    /// </remarks>
    public UmbracoAssemblyLoadContext()
        : base(true)
    {
    }

    // we never load anything directly by assembly name. This method will never be called
    protected override Assembly? Load(AssemblyName assemblyName) => null;
}
