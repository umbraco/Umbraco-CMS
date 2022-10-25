using System.Reflection;

namespace Umbraco.Cms.Infrastructure.Templates.PartialViews;

/// <summary>
/// Populates the Partial View file system using other sources, such as RCL.
/// </summary>
public interface IPartialViewPopulator
{
    /// <summary>
    /// Copies a partial view from the assembly path within the provided assembly, to the file system path. But only if it does not exist yet.
    /// </summary>
    /// <param name="assembly">The assembly to look for embedded resources in.</param>
    /// <param name="embeddedPath">Path to resource as assembly path I.E Umbraco.Cms.Core.EmbeddedResources.</param>
    /// <param name="fileSystemPath">The partial view filesystem path to copy the file to, I.E. /Views/Partials/blockgrid.</param>
    void CopyPartialViewIfNotExists(Assembly assembly, string embeddedPath, string fileSystemPath);

    Assembly GetCoreAssembly();

    string CoreEmbeddedPath { get; }
}
