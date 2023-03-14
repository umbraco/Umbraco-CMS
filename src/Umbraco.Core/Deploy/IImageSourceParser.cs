namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Provides methods to parse image tag sources in property values.
/// </summary>
public interface IImageSourceParser
{
    /// <summary>
    ///     Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>Turns src="/media/..." into src="umb://media/..." and adds the corresponding udi to the dependencies.</remarks>
    string? ToArtifact(string? value, ICollection<Udi> dependencies);

    /// <summary>
    ///     Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <returns>The parsed value.</returns>
    /// <remarks>Turns umb://media/... into /media/....</remarks>
    string? FromArtifact(string? value);
}
