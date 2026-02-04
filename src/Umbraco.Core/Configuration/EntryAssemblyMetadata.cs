using System.Reflection;

namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Provides metadata about the entry assembly by reading assembly attributes.
/// </summary>
internal sealed class EntryAssemblyMetadata : IEntryAssemblyMetadata
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntryAssemblyMetadata" /> class.
    /// </summary>
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

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string InformationalVersion { get; }
}
