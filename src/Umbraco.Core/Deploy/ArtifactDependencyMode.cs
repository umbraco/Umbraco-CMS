namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Indicates the mode of the dependency.
/// </summary>
public enum ArtifactDependencyMode
{
    /// <summary>
    ///     The dependency must match exactly.
    /// </summary>
    Match,

    /// <summary>
    ///     The dependency must exist.
    /// </summary>
    Exist,
}
