using System;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Profile which is shared between Members and Users
    /// </summary>
    public class Profile : IProfile
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

        public object Id { get; set; }

        public string Name { get; set; }

        internal virtual object ProviderUserKey
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