using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Models
{
    public interface IMember : IContentBase, IMembershipUser
    {
        /// <summary>
        /// String alias of the default ContentType
        /// </summary>
        string ContentTypeAlias { get; }

        /// <summary>
        /// Gets the ContentType used by this content object
        /// </summary>
        IMemberType ContentType { get; }

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
