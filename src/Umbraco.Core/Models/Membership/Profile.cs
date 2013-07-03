using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Represents a Profile which is shared between Members and Users
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Profile : TracksChangesEntityBase, IProfile, IRememberBeingDirty
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

        private object _id;
        private string _name;
        private Type _userTypeKey;

        private static readonly PropertyInfo IdSelector = ExpressionHelper.GetPropertyInfo<Profile, object>(x => x.Id);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Profile, string>(x => x.Name);
        private static readonly PropertyInfo UserTypeKeySelector = ExpressionHelper.GetPropertyInfo<Profile, Type>(x => x.ProviderUserKeyType);

        [DataMember]
        public object Id
        {
            get
            {
                return _id;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _id = value;
                    return _id;
                }, _id, IdSelector);
            }
        }

        [DataMember]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

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
        internal Type ProviderUserKeyType
        {
            get
            {
                return _userTypeKey;
            }
            private set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _userTypeKey = value;
                    return _userTypeKey;
                }, _userTypeKey, UserTypeKeySelector);
            }
        }

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