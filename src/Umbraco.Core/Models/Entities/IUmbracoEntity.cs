using System.Collections.Generic;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Represents an entity that can be managed by the entity service.
    /// </summary>
    /// <remarks>
    /// <para>An IUmbracoEntity can be related to another via the IRelationService.</para>
    /// <para>IUmbracoEntities can be retrieved with the IEntityService.</para>
    /// <para>An IUmbracoEntity can participate in notifications.</para>
    /// </remarks>
    public interface IUmbracoEntity : ITreeEntity, IRememberBeingDirty
    {
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
