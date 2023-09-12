using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// Extension methods adding backwards-compatability between <see cref="IDataTypeConfigurationConnector" /> and <see cref="IDataTypeConfigurationConnector2" />.
/// </summary>
/// <remarks>
/// These extension methods will be removed in Umbraco 13.
/// </remarks>
public static class DataTypeConfigurationConnectorExtensions
{
    /// <summary>
    /// Gets the artifact configuration value corresponding to a data type configuration and gather dependencies.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The artifact configuration value.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IDataTypeConfigurationConnector2" />.
    /// </remarks>
    public static string? ToArtifact(this IDataTypeConfigurationConnector connector, IDataType dataType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
            => connector is IDataTypeConfigurationConnector2 connector2
                ? connector2.ToArtifact(dataType, dependencies, contextCache)
                : connector.ToArtifact(dataType, dependencies);

    /// <summary>
    /// Gets the data type configuration corresponding to an artifact configuration value.
    /// </summary>
    /// <param name="connector">The connector.</param>
    /// <param name="dataType">The data type.</param>
    /// <param name="configuration">The artifact configuration value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    /// <remarks>
    /// This extension method tries to make use of the <see cref="IContextCache" /> on types also implementing <see cref="IDataTypeConfigurationConnector2" />.
    /// </remarks>
    public static object? FromArtifact(this IDataTypeConfigurationConnector connector, IDataType dataType, string? configuration, IContextCache contextCache)
            => connector is IDataTypeConfigurationConnector2 connector2
                ? connector2.FromArtifact(dataType, configuration, contextCache)
                : connector.FromArtifact(dataType, configuration);
}
