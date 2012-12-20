using System;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Profile which is shared between Members and Users
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Profile : IProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile()
        {
            ProviderUserKeyType = typeof(int);
        }

        public Profile(object id, string name)
        {
            ProviderUserKeyType = typeof(int);
            Id = id;
            Name = name;
        }

        [DataMember]
        public object Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public virtual object ProviderUserKey
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the type of the provider user key.
        /// </summary>
        /// <value>
        /// The type of the provider user key.
        /// </value>
        [IgnoreDataMember]
        internal Type ProviderUserKeyType { get; private set; }

        /// <summary>
        /// Sets the type of the provider user key.
        /// </summary>
        /// <param name="type">The type.</param>
        internal void SetProviderUserKeyType(Type type)
        {
            ProviderUserKeyType = type;
        }
    }
}