using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides the model creation service.
    /// </summary>
    public interface IPublishedContentModelFactory // fixme rename IFacadeModelFactory
    {
        /// <summary>
        /// Creates a strongly-typed model representing a property set.
        /// </summary>
        /// <param name="set">The original property set.</param>
        /// <returns>The strongly-typed model representing the property set, or the property set
        /// itself it the factory has no model for that content type.</returns>
        IPublishedElement CreateModel(IPublishedElement set);

        /// <summary>
        /// Gets the model type map.
        /// </summary>
        Dictionary<string, Type> ModelTypeMap { get; }

        // fixme
        //
        // ModelFactory.Meta.Model("thing").ClrType (find the our post?)
        //
        // then
        // make a plan to get NestedContent in
        // and an equivalent of Vorto with different syntax
        //
        // then
        // VARIANTS ARCHITECTURE FFS!
    }
}
