using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents the Type for a Backoffice User
    /// </summary>
    /// <remarks>
    /// Should be internal until a proper user/membership implementation
    /// is part of the roadmap.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class UserType : Entity, IUserType
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Permissions { get; set; }
    }
}