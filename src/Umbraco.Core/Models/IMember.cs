using System;
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
    }
}