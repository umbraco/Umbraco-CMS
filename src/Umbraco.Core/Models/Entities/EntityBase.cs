using System.Diagnostics;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Models.Entities;

/// <summary>
///     Provides a base class for entities.
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
    private bool _isKeyAssigned;
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

    /// <summary>
    ///     Gets a value indicating whether the Key can be changed after the entity has identity.
    ///     Override this to return <c>true</c> for entities where Key is derived from other properties (e.g., file path).
    /// </summary>
    protected virtual bool CanChangeKey => false;

    /// <inheritdoc />
    [DataMember]
    public Guid Key
    {
        get
        {
            // if an entity does NOT have a key yet, assign one now
            if (_key == Guid.Empty)
            {
                _key = Guid.NewGuid();
            }

            return _key;
        }
        set
        {
            // The Key (GUID) should be immutable once an entity is persisted to the database.
            // We throw if ALL of these conditions are true:
            //   - !SupportsKeyChange: entity doesn't allow Key changes (file-based entities override this)
            //   - HasIdentity: entity has been persisted (Id != 0)
            //   - _keyIsAssigned: Key was previously set while entity had identity (i.e., loaded from DB or set after save)
            //   - value != Guid.Empty: not resetting the Key (allowed for cloning/identity reset)
            //   - value != _key: actually trying to change to a different value
            if (!CanChangeKey && HasIdentity && _isKeyAssigned && value != Guid.Empty && value != _key)
            {
                throw new InvalidOperationException($"Cannot change the Key of an existing {GetType().Name}.");
            }

            SetPropertyValueAndDetectChanges(value, ref _key, nameof(Key));

            // Track that Key has been assigned, but only when the entity already has identity.
            // This distinction is important:
            //   - Before HasIdentity (new entity): Key can be set freely during setup, _keyIsAssigned stays false
            //   - After HasIdentity (persisted entity): first assignment sets _keyIsAssigned = true, blocking future changes
            // Note: _keyIsAssigned can only be reset via ResetIdentity(), not by setting Key to Guid.Empty.
            // This prevents bypassing the immutability check by setting Key = Guid.Empty then Key = newValue.
            if (HasIdentity && value != Guid.Empty)
            {
                _isKeyAssigned = true;
            }
        }
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
    /// <remarks>
    /// Not serialized as [DataMember] because it's derived from Id.
    /// When Id is deserialized, the setter sets _hasIdentity correctly.
    /// </remarks>
    public virtual bool HasIdentity => _hasIdentity;

    /// <summary>
    ///     Resets the entity identity.
    /// </summary>
    public virtual void ResetIdentity()
    {
        _id = default;
        _key = Guid.Empty;
        _isKeyAssigned = false;
        _hasIdentity = false;
    }

    /// <summary>
    ///     Called after deserialization to restore the _keyIsAssigned flag.
    ///     This ensures Key immutability is enforced regardless of property deserialization order.
    /// </summary>
    [OnDeserialized]
    private void OnDeserialized(StreamingContext context) =>
        _isKeyAssigned = HasIdentity && _key != Guid.Empty;

    /// <summary>
    ///     Determines whether the specified <see cref="EntityBase" /> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="EntityBase" /> to compare with this instance.</param>
    /// <returns><c>true</c> if the specified entity is equal to this instance; otherwise, <c>false</c>.</returns>
    public virtual bool Equals(EntityBase? other) =>
        other != null && (ReferenceEquals(this, other) || SameIdentityAs(other));

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj != null && (ReferenceEquals(this, obj) || SameIdentityAs(obj as EntityBase));

    /// <inheritdoc />
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

    /// <summary>
    ///     Determines whether this entity has the same identity as another entity.
    /// </summary>
    /// <param name="other">The other entity to compare.</param>
    /// <returns><c>true</c> if the entities have the same identity; otherwise, <c>false</c>.</returns>
    private bool SameIdentityAs(EntityBase? other)
    {
        if (other == null)
        {
            return false;
        }

        // same identity if
        // - same object (reference equals)
        // - or same CLR type, both have identities, and they are identical
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType() && HasIdentity && other.HasIdentity && Id == other.Id;
    }

    /// <inheritdoc />
    public object DeepClone()
    {
        // memberwise-clone (ie shallow clone) the entity
        Guid unused = Key; // ensure that 'this' has a key, before cloning
        var clone = (EntityBase)MemberwiseClone();

#if DEBUG_MODEL
            clone.InstanceId = Guid.NewGuid();
#endif

        // disable change tracking while we deep clone IDeepCloneable properties
        clone.DisableChangeTracking();

        // deep clone ref properties that are IDeepCloneable
        DeepCloneHelper.DeepCloneRefProperties(this, clone);

        PerformDeepClone(clone);

        // clear changes (ensures the clone has its own dictionaries)
        clone.ResetDirtyProperties(false);

        // re-enable change tracking
        clone.EnableChangeTracking();

        return clone;
    }

    /// <summary>
    ///     Used by inheritors to modify the DeepCloning logic
    /// </summary>
    /// <param name="clone"></param>
    protected virtual void PerformDeepClone(object clone)
    {
    }
}
