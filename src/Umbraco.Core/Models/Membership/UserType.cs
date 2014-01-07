using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The set of default permissions for the user type
        /// </summary>
        /// <remarks>
        /// By default each permission is simply a single char but we've made this an enumerable{string} to support a more flexible permissions structure in the future.
        /// </remarks>
        [DataMember]
        public IEnumerable<string> Permissions { get; set; }
    }
}