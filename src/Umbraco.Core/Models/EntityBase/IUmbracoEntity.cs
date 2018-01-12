using System;
using System.Collections.Generic;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// Represents fixme what exactly?
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
        IDictionary<string, object> AdditionalData { get; }

        // fixme AdditionalData is never null, then we need a HasAdditionalData for checking values?

        ///// <summary>
        ///// Gets a value indicating whether this entity has children.
        ///// </summary>
        //bool HasChildren { get; }

        ///// <summary>
        ///// Gets the node object type of the entity.
        ///// </summary>
        //Guid NodeObjectType { get; }

    }
}
