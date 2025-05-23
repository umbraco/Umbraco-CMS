using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Defines methods that can convert data type configuration to and from an environment-agnostic string.
/// </summary>
/// <remarks>
/// Configuration may contain values such as content identifiers, that would be local to one environment, and need to be converted in order to be deployed.
/// It can also contain references to other deployable artifacts, that need to be tracked as dependencies.
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
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the artifact configuration value.
    /// </returns>
    Task<string?> ToArtifactAsync(
        IDataType dataType,
        ICollection<ArtifactDependency> dependencies,
        IContextCache contextCache,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the data type configuration corresponding to an artifact configuration value.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="configuration">The artifact configuration value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the data type configuration.
    /// </returns>
    Task<IDictionary<string, object>> FromArtifactAsync(
        IDataType dataType,
        string? configuration,
        IContextCache contextCache,
        CancellationToken cancellationToken = default);
}
