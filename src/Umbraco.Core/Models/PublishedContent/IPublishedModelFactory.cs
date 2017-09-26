using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides the published model creation service.
    /// </summary>
    public interface IPublishedModelFactory
    {
        /// <summary>
        /// Creates a strongly-typed model representing a published element.
        /// </summary>
        /// <param name="element">The original published element.</param>
        /// <returns>The strongly-typed model representing the published element, or the published element
        /// itself it the factory has no model for the corresponding element type.</returns>
        IPublishedElement CreateModel(IPublishedElement element);

        /// <summary>
        /// Gets the model type map.
        /// </summary>
        /// <remarks>The model type map maps element type aliases to actual Clr types.</remarks>
        Dictionary<string, Type> ModelTypeMap { get; }
    }
}
