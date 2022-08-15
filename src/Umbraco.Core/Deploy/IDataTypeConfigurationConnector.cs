using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
///     Defines methods that can convert data type configuration to / from an environment-agnostic string.
/// </summary>
/// <remarks>
///     Configuration may contain values such as content identifiers, that would be local
///     to one environment, and need to be converted in order to be deployed.
/// </remarks>
[SuppressMessage(
    "ReSharper",
    "UnusedMember.Global",
    Justification = "This is actual only used by Deploy, but we don't want third parties to have references on deploy, that's why this interface is part of core.")]
public interface IDataTypeConfigurationConnector
{
    /// <summary>
    ///     Gets the property editor aliases that the value converter supports by default.
    /// </summary>
    IEnumerable<string> PropertyEditorAliases { get; }

    /// <summary>
    ///     Gets the artifact datatype configuration corresponding to the actual datatype configuration.
    /// </summary>
    /// <param name="dataType">The datatype.</param>
    /// <param name="dependencies">The dependencies.</param>
    string? ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies);

    /// <summary>
    ///     Gets the actual datatype configuration corresponding to the artifact configuration.
    /// </summary>
    /// <param name="dataType">The datatype.</param>
    /// <param name="configuration">The artifact configuration.</param>
    object? FromArtifact(IDataType dataType, string? configuration);
}
