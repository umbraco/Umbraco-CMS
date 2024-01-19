namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents an artifact ie an object that can be transfered between environments.
/// </summary>
public interface IArtifact : IArtifactSignature
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the alias.
    /// </summary>
    /// <value>
    /// The alias.
    /// </value>
    string? Alias { get; }
}
