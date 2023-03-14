namespace Umbraco.Cms.Core.Deploy;

public interface IMacroParser
{
    /// <summary>
    ///     Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">Property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>Parsed value.</returns>
    string? ToArtifact(string? value, ICollection<Udi> dependencies);

    /// <summary>
    ///     Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">Artifact property value.</param>
    /// <returns>Parsed value.</returns>
    string? FromArtifact(string? value);

    /// <summary>
    ///     Tries to replace the value of the attribute/parameter with a value containing a converted identifier.
    /// </summary>
    /// <param name="value">Value to attempt to convert</param>
    /// <param name="editorAlias">Alias of the editor used for the parameter</param>
    /// <param name="dependencies">Collection to add dependencies to when performing ToArtifact</param>
    /// <param name="direction">Indicates which action is being performed (to or from artifact)</param>
    /// <returns>Value with converted identifiers</returns>
    string ReplaceAttributeValue(string value, string editorAlias, ICollection<Udi> dependencies, Direction direction);
}
