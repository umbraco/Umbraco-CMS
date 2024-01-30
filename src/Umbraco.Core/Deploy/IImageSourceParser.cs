namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Provides methods to parse image tag sources in property values.
/// </summary>
public interface IImageSourceParser
{
    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns src="/media/..." into src="umb://media/..." and adds the corresponding udi to the dependencies.
    /// </remarks>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string ToArtifact(string value, ICollection<Udi> dependencies);

    /// <summary>
    /// Parses an Umbraco property value and produces an artifact property value.
    /// </summary>
    /// <param name="value">The property value.</param>
    /// <param name="dependencies">A list of dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns src="/media/..." into src="umb://media/..." and adds the corresponding udi to the dependencies.
    /// </remarks>
    string ToArtifact(string value, ICollection<Udi> dependencies, IContextCache contextCache)
#pragma warning disable CS0618 // Type or member is obsolete
        => ToArtifact(value, dependencies);
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns umb://media/... into /media/....
    /// </remarks>
    [Obsolete("Please use the overload taking all parameters. This method will be removed in Umbraco 14.")]
    string FromArtifact(string value);

    /// <summary>
    /// Parses an artifact property value and produces an Umbraco property value.
    /// </summary>
    /// <param name="value">The artifact property value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The parsed value.
    /// </returns>
    /// <remarks>
    /// Turns umb://media/... into /media/....
    /// </remarks>
    string FromArtifact(string value, IContextCache contextCache)
#pragma warning disable CS0618 // Type or member is obsolete
        => FromArtifact(value);
#pragma warning restore CS0618 // Type or member is obsolete
}
