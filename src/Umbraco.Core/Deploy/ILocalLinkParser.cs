namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Provides methods to parse local link tags in property values.
/// </summary>
public interface ILocalLinkParser
{
    /// <summary>
    ///     Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>
    ///     Turns {{localLink:1234}} into {{localLink:umb://{type}/{id}}} and adds the corresponding udi to the
    ///     dependencies.
    /// </remarks>
    string ToArtifact(string value, ICollection<Udi> dependencies);

    /// <summary>
    ///     Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>Turns {{localLink:umb://{type}/{id}}} into {{localLink:1234}}.</remarks>
    string FromArtifact(string value);
}
