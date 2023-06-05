﻿using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <inheritdoc />
/// <remarks>
/// This interface will be merged back into <see cref="IDataTypeConfigurationConnector" /> and removed in Umbraco 13.
/// </remarks>
public interface IDataTypeConfigurationConnector2 : IDataTypeConfigurationConnector
{
    /// <summary>
    /// Gets the artifact configuration value corresponding to a data type configuration and gather dependencies.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <returns>
    /// The artifact configuration value.
    /// </returns>
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    string? IDataTypeConfigurationConnector.ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies)
        => ToArtifact(dataType, dependencies, PassThroughCache.Instance);

    /// <summary>
    /// Gets the artifact configuration value corresponding to a data type configuration and gather dependencies.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="dependencies">The dependencies.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The artifact configuration value.
    /// </returns>
    string? ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache);

    /// <summary>
    /// Gets the data type configuration corresponding to an artifact configuration value.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="configuration">The artifact configuration value.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    [Obsolete($"Use the overload accepting {nameof(IContextCache)} instead. This overload will be removed in Umbraco 13.")]
    object? IDataTypeConfigurationConnector.FromArtifact(IDataType dataType, string? configuration)
        => FromArtifact(dataType, configuration, PassThroughCache.Instance);

    /// <summary>
    /// Gets the data type configuration corresponding to an artifact configuration value.
    /// </summary>
    /// <param name="dataType">The data type.</param>
    /// <param name="configuration">The artifact configuration value.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The data type configuration.
    /// </returns>
    object? FromArtifact(IDataType dataType, string? configuration, IContextCache contextCache);
}
