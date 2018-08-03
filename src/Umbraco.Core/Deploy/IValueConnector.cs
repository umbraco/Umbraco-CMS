using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Deploy
{
    /// <summary>
    /// Defines methods that can convert a property value to / from an environment-agnostic string.
    /// </summary>
    /// <remarks>Property values may contain values such as content identifiers, that would be local
    /// to one environment, and need to be converted in order to be deployed. Connectors also deal
    /// with serializing to / from string.</remarks>
    public interface IValueConnector
    {
        /// <summary>
        /// Gets the property editor aliases that the value converter supports by default.
        /// </summary>
        IEnumerable<string> PropertyEditorAliases { get; }

        /// <summary>
        /// Gets the deploy property corresponding to a content property.
        /// </summary>
        /// <param name="property">The content property.</param>
        /// <param name="dependencies">The content dependencies.</param>
        /// <returns>The deploy property value.</returns>
        string GetValue(Property property, ICollection<ArtifactDependency> dependencies);

        /// <summary>
        /// Sets a content property value using a deploy property.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The deploy property value.</param>
        void SetValue(IContentBase content, string alias, string value);
    }
}