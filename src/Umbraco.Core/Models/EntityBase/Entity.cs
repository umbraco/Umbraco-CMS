using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// Base Abstract Entity
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public abstract class Entity : IEntity, ICanBeDirty
    {
        private bool _hasIdentity;
        private int? _hash;
        private int _id;
        private Guid _key;

        /// <summary>
        /// Integer Id
        /// </summary>
        [DataMember]
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                HasIdentity = true;
            }
        }

        /// <summary>
        /// Guid based Id
        /// </summary>
        /// <remarks>The key is currectly used to store the Unique Id from the 
        /// umbracoNode table, which many of the entities are based on.</remarks>
        [DataMember]
        public Guid Key
        {
            get
            {
                if (_key == Guid.Empty)
                    return Id.ToGuid();

                return _key;
            }
            set { _key = value; }
        }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        [DataMember]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Gets or sets the Modified Date
        /// </summary>
        [DataMember]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method to call on a property setter.
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
        {
            _propertyChangedInfo[propertyInfo.Name] = true;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyInfo.Name));
            }
        }

        protected void ResetIdentity()
        {
            _hasIdentity = false;
            _id = 0;
        }

        /// <summary>
        /// Method to call on entity saved when first added
        /// </summary>
        internal virtual void AddingEntity()
        {
            CreateDate = DateTime.UtcNow;
            UpdateDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Method to call on entity saved/updated
        /// </summary>
        internal virtual void UpdatingEntity()
        {
            UpdateDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Tracks the properties that have changed
        /// </summary>
        private readonly IDictionary<string, bool> _propertyChangedInfo = new Dictionary<string, bool>();

        /// <summary>
        /// Returns true if the property referenced by the name has been changed on the class
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsPropertyDirty(string propertyName)
        {
            return _propertyChangedInfo.Any(x => x.Key == propertyName);
        }

        /// <summary>
        /// Returns true if any properties have been changed on the class
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return _propertyChangedInfo.Any();
        }

        /// <summary>
        /// Resets dirty properties by clearing the dictionary used to track changes.
        /// </summary>
        internal void ResetDirtyProperties()
        {
            _propertyChangedInfo.Clear();
        }

        /// <summary>
        /// Indicates whether the current entity has an identity, eg. Id.
        /// </summary>
        public virtual bool HasIdentity
        {
            get
            {
                return _hasIdentity;
            }
            protected set
            {
                _hasIdentity = value;
            }
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (ReferenceEquals(null, left))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

        public virtual bool SameIdentityAs(IEntity other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SameIdentityAs(other as Entity);
        }

        public virtual bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return SameIdentityAs(other);
        }

        public virtual Type GetRealType()
        {
            return GetType();
        }

        public virtual bool SameIdentityAs(Entity other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() == other.GetRealType() && HasIdentity && other.HasIdentity)
                return other.Id.Equals(Id);

            return false;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return SameIdentityAs(obj as IEntity);
        }

        public override int GetHashCode()
        {
            if (!_hash.HasValue)
                _hash = !HasIdentity ? new int?(base.GetHashCode()) : new int?(Id.GetHashCode() * 397 ^ GetType().GetHashCode());
            return _hash.Value;
        }
    }
}