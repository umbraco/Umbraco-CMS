using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    internal interface IMember : IMembershipUser, IMemberProfile
    {
        new int Id { get; set; }

        /// <summary>
        /// Guid Id of the curent Version
        /// </summary>
        [DataMember]
        Guid Version { get; }

        /// <summary>
        /// String alias of the default ContentType
        /// </summary>
        [DataMember]
        string ContentTypeAlias { get; }
    }

    internal interface IMemberProfile : IProfile
    {

    }
}