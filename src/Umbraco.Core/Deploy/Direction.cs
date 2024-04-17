namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Represents the direction when replacing an attribute value while parsing macros.
/// </summary>
public enum Direction
{
    /// <summary>
    /// Replacing an attribute value while converting to an artifact.
    /// </summary>
    ToArtifact,
    /// <summary>
    /// Replacing an attribute value while converting from an artifact.
    /// </summary>
    FromArtifact,
}
