namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Defines a value validator that can be referenced in a manifest.
/// </summary>
/// <remarks>If the manifest can be configured, then it should expose a Configuration property.</remarks>
public interface IManifestValueValidator : IValueValidator
{
    /// <summary>
    ///     Gets the name of the validator.
    /// </summary>
    string ValidationName { get; }
}
