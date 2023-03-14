using System.Reflection;

namespace Umbraco.Cms.Core.Configuration;

internal class EntryAssemblyMetadata : IEntryAssemblyMetadata
{
    public EntryAssemblyMetadata()
    {
        var entryAssembly = Assembly.GetEntryAssembly();

        Name = entryAssembly
            ?.GetName()
            ?.Name ?? string.Empty;

        InformationalVersion = entryAssembly
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? string.Empty;
    }

    public string Name { get; }

    public string InformationalVersion { get; }
}
