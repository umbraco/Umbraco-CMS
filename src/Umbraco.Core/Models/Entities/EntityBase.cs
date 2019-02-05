using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.Entities
{
    /// <summary>
    /// Provides a base class for entities.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {" + nameof(Id) + "}")]
    public abstract class EntityBase : BeingDirtyBase, IEntity
    {
#if DEBUG_MODEL
        public Guid InstanceId = Guid.NewGuid();
#endif

        private bool _hasIdentity;
        private int _id;
        private Guid _key;
        private DateTime _createDate;
        private DateTime _updateDate;

        /// <inheritdoc />
        [DataMember]
        public int Id
        {
            get => _id;
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _id, nameof(Id));
                _hasIdentity = value != 0;
            }
        }

        /// <inheritdoc />
        [DataMember]
        public Guid Key
        {
            get
            {
                // if an entity does NOT have a key yet, assign one now
                if (_key == Guid.Empty)
                    _key = Guid.NewGuid();
                return _key;
            }
            set => SetPropertyValueAndDetectChanges(value, ref _key, nameof(Key));
        }

        /// <inheritdoc />
        [DataMember]
        public DateTime CreateDate
        {
            get => _createDate;
            set => SetPropertyValueAndDetectChanges(value, ref _createDate, nameof(CreateDate));
        }

        /// <inheritdoc />
        [DataMember]
        public DateTime UpdateDate
        {
            get => _updateDate;
            set => SetPropertyValueAndDetectChanges(value, ref _updateDate, nameof(UpdateDate));
        }

        /// <inheritdoc />
        [DataMember]
        public DateTime? DeleteDate { get; set; } // no change tracking - not persisted

        /// <inheritdoc />
        [DataMember]
        public virtual bool HasIdentity => _hasIdentity;

        /// <summary>
        /// Resets the entity identity.
        /// </summary>
        internal virtual void ResetIdentity()
        {
            _id = default;
            _key = Guid.Empty;
            _hasIdentity = false;
        }

        /// <summary>
        /// Updates the entity when it is being saved for the first time.
        /// </summary>
        internal virtual void AddingEntity()
        {
            var now = DateTime.Now;

            // set the create and update dates, if not already set
            if (IsPropertyDirty("CreateDate") == false || _createDate == default)
                CreateDate = now;
            if (IsPropertyDirty("UpdateDate") == false || _updateDate == default)
                UpdateDate = now;
        }

        /// <summary>
        /// Updates the entity when it is being saved.
        /// </summary>
        internal virtual void UpdatingEntity()
        {
            var now = DateTime.Now;

            // just in case
            if (_createDate == default)
                CreateDate = now;

            // set the update date if not already set
            if (IsPropertyDirty("UpdateDate") == false || _updateDate == default)
                UpdateDate = now;
        }

        public virtual bool Equals(EntityBase other)
        {
            return other != null && (ReferenceEquals(this, other) || SameIdentityAs(other));
        }

        public override bool Equals(object obj)
        {
            return obj != null && (ReferenceEquals(this, obj) || SameIdentityAs(obj as EntityBase));
        }

        private bool SameIdentityAs(EntityBase other)
        {
            if (other == null) return false;

            // same identity if
            // - same object (reference equals)
            // - or same CLR type, both have identities, and they are identical

            if (ReferenceEquals(this, other))
                return true;

            return GetType() == other.GetType() && HasIdentity && other.HasIdentity && Id == other.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HasIdentity.GetHashCode();
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ GetType().GetHashCode();
                return hashCode;
            }
        }

        public object DeepClone()
        {
            // memberwise-clone (ie shallow clone) the entity
            var unused = Key; // ensure that 'this' has a key, before cloning
            var clone = (EntityBase) MemberwiseClone();

#if DEBUG_MODEL
            clone.InstanceId = Guid.NewGuid();
#endif

            //disable change tracking while we deep clone IDeepCloneable properties
            clone.DisableChangeTracking();

            // deep clone ref properties that are IDeepCloneable
            DeepCloneHelper.DeepCloneRefProperties(this, clone);

            PerformDeepClone(clone);

            // clear changes (ensures the clone has its own dictionaries)
            clone.ResetDirtyProperties(false);

            //re-enable change tracking
            clone.EnableChangeTracking();

            return clone;
        }

        /// <summary>
        /// Used by inheritors to modify the DeepCloning logic
        /// </summary>
        /// <param name="clone"></param>
        protected virtual void PerformDeepClone(object clone)
        {
        }
    }
}
