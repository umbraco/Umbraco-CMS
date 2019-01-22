﻿using System;
using System.Collections;

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
        /// Creates a List{T} of a strongly-typed model for a model type alias.
        /// </summary>
        /// <param name="alias">The model type alias.</param>
        /// <returns>A List{T} of the strongly-typed model, exposed as an IList.</returns>
        IList CreateModelList(string alias);

        /// <summary>
        /// Maps a common language runtime type that may contain model types, to an actual common language runtime type.
        /// </summary>
        /// <param name="type">The common language runtime type.</param>
        /// <returns>The actual common language runtime type.</returns>
        /// <remarks>See <seealso cref="ModelType"/> for more details.</remarks>
        Type MapModelType(Type type);
    }
}
