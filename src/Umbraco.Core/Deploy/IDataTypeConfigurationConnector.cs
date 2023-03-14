using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Defines methods that can convert data type configuration to / from an environment-agnostic string.
/// </summary>
/// <remarks>
/// Configuration may contain values such as content identifiers, that would be local
/// to one environment, and need to be converted in order to be deployed.
/// </remarks>
public interface IDataTypeConfigurationConnector
{
    /// <summary>
    /// Gets the property editor aliases that the value converter supports by default.
    /// </summary>
    /// <value>
    /// The property editor aliases.
    /// </value>
    IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    /// Gets the artifact configuration value corresponding to a data type configuration and gather dependencies.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <returns>
    /// The artifact configuration value.
    /// </returns>
    [Obsolete($"Implement {nameof(IDataTypeConfigurationConnector2)} and use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    string? ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies);

    /// <summary>
    /// Gets the data type configuration corresponding to an artifact configuration value.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="configuration">The artifact configuration value.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    [Obsolete($"Implement {nameof(IDataTypeConfigurationConnector2)} and use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    object? FromArtifact(IDataType dataType, string? configuration);
}
