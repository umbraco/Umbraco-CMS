using System.Collections.Generic;

namespace Umbraco.Core.Deploy
{
    /// <summary>
    /// Defines methods that can convert a preValue to / from an environment-agnostic string.
    /// </summary>
    /// <remarks>PreValues may contain values such as content identifiers, that would be local
    /// to one environment, and need to be converted in order to be deployed.</remarks>
    public interface IPreValueConnector // fixme this needs to change really
    {
        /// <summary>
        /// Gets the property editor aliases that the value converter supports by default.
        /// </summary>
        IEnumerable<string> PropertyEditorAliases { get; }

        /// <summary>
        /// Gets the environment-agnostic preValues corresponding to environment-specific preValues.
        /// </summary>
        /// <param name="preValues">The environment-specific preValues.</param>
        /// <param name="dependencies">The dependencies.</param>
        /// <returns></returns>
        IDictionary<string, string> ConvertToDeploy(IDictionary<string, string> preValues, ICollection<ArtifactDependency> dependencies);

        /// <summary>
        /// Gets the environment-specific preValues corresponding to environment-agnostic preValues.
        /// </summary>
        /// <param name="preValues">The environment-agnostic preValues.</param>
        /// <returns></returns>
        IDictionary<string, string> ConvertToLocalEnvironment(IDictionary<string, string> preValues);
    }
}
