using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Umbraco.Core.Deploy
{
    /// <summary>
    /// Defines methods that can convert a configuration to / from an environment-agnostic string.
    /// </summary>
    /// <remarks>Configuration may contain values such as content identifiers, that would be local
    /// to one environment, and need to be converted in order to be deployed.</remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is actual only used by Deploy, but we dont want third parties to have references on deploy, thats why this interface is part of core.")]
    public interface IDataTypeConfigurationConnector
    {
        /// <summary>
        /// Gets the property editor aliases that the value converter supports by default.
        /// </summary>
        IEnumerable<string> PropertyEditorAliases { get; }

        /// <summary>
        /// Gets the environment-agnostic configuration corresponding to environment-specific configuration.
        /// </summary>
        /// <param name="configuration">The environment-specific configuration</param>
        /// <param name="dependencies">The dependencies.</param>
        /// <returns>A serializable  <see cref="IDictionary{TKey,TValue}"/> containing editor key and serialized configuration </returns>
        IDictionary<string, string> ToArtifact(IDictionary<string, object> configuration, ICollection<ArtifactDependency> dependencies);

        /// <summary>
        /// Gets the environment-specific configurations corresponding to environment-agnostic configurations.
        /// </summary>
        /// <param name="configuration">The environment-agnostic configuration.</param>
        /// <returns>The configuration objected</returns>
        IDictionary<string, object> FromArtifact(IDictionary<string, string> configuration);
    }
}
