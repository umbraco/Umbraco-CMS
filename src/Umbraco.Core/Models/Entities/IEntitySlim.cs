using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Represents a lightweight entity, managed by the entity service.
    /// </summary>
    public interface IEntitySlim : IUmbracoEntity
    {
        /// <summary>
        /// Gets or sets the entity object type.
        /// </summary>
        Guid NodeObjectType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has children.
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// Gets a value indicating whether the entity is a container.
        /// </summary>
        bool IsContainer { get; }

        /// <summary>
        /// Gets additional data for this entity.
        /// </summary>
        /// <remarks>Can be empty, but never null. To avoid allocating, do not
        /// test for emptyness, but use <see cref="HasAdditionalData"/> instead.</remarks>
        IDictionary<string, object> AdditionalData { get; }

        /// <summary>
        /// Determines whether this entity has additional data.
        /// </summary>
        /// <remarks>Use this property to check for additional data without
        /// getting <see cref="AdditionalData"/>, to avoid allocating.</remarks>
        bool HasAdditionalData { get; }
    }
}
