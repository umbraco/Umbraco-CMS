namespace Umbraco.Cms.Core.Configuration;

/// <summary>
///     Provides metadata about the entry assembly.
/// </summary>
public interface IEntryAssemblyMetadata
{
    /// <summary>
    ///     Gets the Name of entry assembly.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the InformationalVersion string for entry assembly.
    /// </summary>
    public string InformationalVersion { get; }
}
