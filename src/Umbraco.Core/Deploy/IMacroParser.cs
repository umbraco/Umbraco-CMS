namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Provides methods to parse macro tags in property values.
/// </summary>
public interface IMacroParser
{
    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">Property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>
    /// Parsed value.
    /// </returns>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string ToArtifact(string value, ICollection<Udi> dependencies);

    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">Property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// Parsed value.
    /// </returns>
    string ToArtifact(string value, ICollection<Udi> dependencies, IContextCache contextCache)
#pragma warning disable CS0618 // Type or member is obsolete
        => ToArtifact(value, dependencies);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">Artifact property value.</param>
    /// <returns>
    /// Parsed value.
    /// </returns>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string FromArtifact(string value);

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">Artifact property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// Parsed value.
    /// </returns>
    string FromArtifact(string value, IContextCache contextCache)
#pragma warning disable CS0618 // Type or member is obsolete
        => FromArtifact(value);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Tries to replace the value of the attribute/parameter with a value containing a converted identifier.
    /// </summary>
    /// <param name="value">Value to attempt to convert</param>
    /// <param name="editorAlias">Alias of the editor used for the parameter</param>
    /// <param name="dependencies">Collection to add dependencies to when performing ToArtifact</param>
    /// <param name="direction">Indicates which action is being performed (to or from artifact)</param>
    /// <returns>
    /// Value with converted identifiers
    /// </returns>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string ReplaceAttributeValue(string value, string editorAlias, ICollection<Udi> dependencies, Direction direction);

    /// <summary>
    /// Tries to replace the value of the attribute/parameter with a value containing a converted identifier.
    /// </summary>
    /// <param name="value">Value to attempt to convert</param>
    /// <param name="editorAlias">Alias of the editor used for the parameter</param>
    /// <param name="dependencies">Collection to add dependencies to when performing ToArtifact</param>
    /// <param name="direction">Indicates which action is being performed (to or from artifact)</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// Value with converted identifiers
    /// </returns>
    string ReplaceAttributeValue(string value, string editorAlias, ICollection<Udi> dependencies, Direction direction, IContextCache contextCache)
#pragma warning disable CS0618 // Type or member is obsolete
        => ReplaceAttributeValue(value, editorAlias, dependencies, direction);
#pragma warning restore CS0618 // Type or member is obsolete
}
