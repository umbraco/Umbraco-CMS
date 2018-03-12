﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Core.Models.EntityBase
{
    /// <summary>
    /// Base Abstract Entity
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    [DebuggerDisplay("Id: {Id}")]
    public abstract class Entity : TracksChangesEntityBase, IEntity, IRememberBeingDirty, ICanBeDirty
    {
        private bool _hasIdentity;
        private int _id;
        private Guid _key;
        private DateTime _createDate;
        private DateTime _updateDate;
        private bool _wasCancelled;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo IdSelector = ExpressionHelper.GetPropertyInfo<Entity, int>(x => x.Id);
            public readonly PropertyInfo KeySelector = ExpressionHelper.GetPropertyInfo<Entity, Guid>(x => x.Key);
            public readonly PropertyInfo CreateDateSelector = ExpressionHelper.GetPropertyInfo<Entity, DateTime>(x => x.CreateDate);
            public readonly PropertyInfo UpdateDateSelector = ExpressionHelper.GetPropertyInfo<Entity, DateTime>(x => x.UpdateDate);
            public readonly PropertyInfo HasIdentitySelector = ExpressionHelper.GetPropertyInfo<Entity, bool>(x => x.HasIdentity);
            public readonly PropertyInfo WasCancelledSelector = ExpressionHelper.GetPropertyInfo<Entity, bool>(x => x.WasCancelled);
        }

        /// <summary>
        /// Integer Id
        /// </summary>
        [DataMember]
        public int Id
        {
            get { return _id; }
            set
            {
                SetPropertyValueAndDetectChanges(value, ref _id, Ps.Value.IdSelector);
                HasIdentity = true; //set the has Identity
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
                // if an entity does NOT have a UniqueId yet, assign one now
                if (_key == Guid.Empty)
                    _key = Guid.NewGuid();
                return _key;
            }
            set { SetPropertyValueAndDetectChanges(value, ref _key, Ps.Value.KeySelector); }
        }

        /// <summary>
        /// Gets or sets the Created Date
        /// </summary>
        [DataMember]
        public DateTime CreateDate
        {
            get { return _createDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _createDate, Ps.Value.CreateDateSelector); }            
        }

        /// <summary>
        /// Gets or sets the WasCancelled flag, which is used to track
        /// whether some action against an entity was cancelled through some event.
        /// This only exists so we have a way to check if an event was cancelled through
        /// the new api, which also needs to take effect in the legacy api.
        /// </summary>
        [IgnoreDataMember]
        [Obsolete("Anytime there's a cancellable method it needs to return an Attempt so we know the outcome instead of this hack, not all services have been updated to use this though yet.")]
        internal bool WasCancelled
        {
            get { return _wasCancelled; }
            set { SetPropertyValueAndDetectChanges(value, ref _wasCancelled, Ps.Value.WasCancelledSelector); }            
        }

        /// <summary>
        /// Gets or sets the Modified Date
        /// </summary>
        [DataMember]
        public DateTime UpdateDate
        {
            get { return _updateDate; }
            set { SetPropertyValueAndDetectChanges(value, ref _updateDate, Ps.Value.UpdateDateSelector); }           
        }

        [IgnoreDataMember]
        public DateTime? DeletedDate { get; set; }

        internal virtual void ResetIdentity()
        {
            _hasIdentity = false;
            _id = default(int);
            _key = Guid.Empty;
        }

        /// <summary>
        /// Method to call on entity saved when first added
        /// </summary>
        internal virtual void AddingEntity()
        {
            if (IsPropertyDirty("CreateDate") == false || _createDate == default(DateTime))
                CreateDate = DateTime.Now;
            if (IsPropertyDirty("UpdateDate") == false || _updateDate == default(DateTime))
                UpdateDate = DateTime.Now;
        }

        /// <summary>
        /// Method to call on entity saved/updated
        /// </summary>
        internal virtual void UpdatingEntity()
        {
            if (IsPropertyDirty("UpdateDate") == false || _updateDate == default(DateTime))
                UpdateDate = DateTime.Now;

            //this is just in case
            if (_createDate == default(DateTime))
                CreateDate = DateTime.Now;
        }

        /// <summary>
        /// Indicates whether the current entity has an identity, eg. Id.
        /// </summary>
        [DataMember]
        public virtual bool HasIdentity
        {
            get
            {
                return _hasIdentity;
            }
            protected set { SetPropertyValueAndDetectChanges(value, ref _hasIdentity, Ps.Value.HasIdentitySelector); }
        }

        //TODO: Make this NOT virtual or even exist really!
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

        //TODO: Make this NOT virtual or even exist really!
        public virtual Type GetRealType()
        {
            return GetType();
        }

        //TODO: Make this NOT virtual or even exist really!
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
            unchecked
            {
                var hashCode = HasIdentity.GetHashCode();
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ GetType().GetHashCode();
                return hashCode;
            }
        }

        public virtual object DeepClone()
        {
            //Memberwise clone on Entity will work since it doesn't have any deep elements
            // for any sub class this will work for standard properties as well that aren't complex object's themselves.
            var ignored = this.Key; // ensure that 'this' has a key, before cloning
            var clone = (Entity)MemberwiseClone();
            //ensure the clone has it's own dictionaries
            clone.ResetChangeTrackingCollections();
            //turn off change tracking
            clone.DisableChangeTracking();
            //Automatically deep clone ref properties that are IDeepCloneable
            DeepCloneHelper.DeepCloneRefProperties(this, clone);
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}