// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Collections;

public abstract class Item : IEntity, ICanBeDirty
{
    /// <summary>
    ///     Tracks the properties that have changed
    /// </summary>
    private readonly IDictionary<string, bool> _propertyChangedInfo;

    private bool _hasIdentity;
    private int _id;
    private Guid _key;
    private bool _withChanges = true; // should we track changes?

    protected Item() => _propertyChangedInfo = new Dictionary<string, bool>();

    /// <summary>
    ///     Gets or sets a value indicating whether some action against an entity was cancelled through some event.
    ///     This only exists so we have a way to check if an event was cancelled through
    ///     the new api, which also needs to take effect in the legacy api.
    /// </summary>
    [IgnoreDataMember]
    internal bool WasCancelled { get; set; }

    /// <summary>
    ///     Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///     Indicates whether a specific property on the current entity is dirty.
    /// </summary>
    /// <param name="propertyName">Name of the property to check</param>
    /// <returns>True if Property is dirty, otherwise False</returns>
    public virtual bool IsPropertyDirty(string propertyName) => _propertyChangedInfo.Any(x => x.Key == propertyName);

    public virtual IEnumerable<string> GetDirtyProperties() => _propertyChangedInfo.Keys;

    /// <summary>
    ///     Indicates whether the current entity is dirty.
    /// </summary>
    /// <returns>True if entity is dirty, otherwise False</returns>
    public virtual bool IsDirty() => _propertyChangedInfo.Any();

    /// <summary>
    ///     Resets dirty properties by clearing the dictionary used to track changes.
    /// </summary>
    /// <remarks>
    ///     Please note that resetting the dirty properties could potentially
    ///     obstruct the saving of a new or updated entity.
    /// </remarks>
    public virtual void ResetDirtyProperties() => _propertyChangedInfo.Clear();

    /// <summary>
    ///     Disables change tracking.
    /// </summary>
    public void DisableChangeTracking() => _withChanges = false;

    /// <summary>
    ///     Enables change tracking.
    /// </summary>
    public void EnableChangeTracking() => _withChanges = true;

    /// <summary>
    ///     Gets or sets the integer Id
    /// </summary>
    [DataMember]
    public int Id
    {
        get => _id;
        set
        {
            _id = value;
            HasIdentity = true;
        }
    }

    /// <summary>
    ///     Gets or sets the Guid based Id
    /// </summary>
    /// <remarks>
    ///     The key is currectly used to store the Unique Id from the
    ///     umbracoNode table, which many of the entities are based on.
    /// </remarks>
    [DataMember]
    public Guid Key
    {
        get => _key == Guid.Empty ? _id.ToGuid() : _key;
        set => _key = value;
    }

    /// <summary>
    ///     Gets or sets the Created Date
    /// </summary>
    [DataMember]
    public DateTime CreateDate { get; set; }

    /// <summary>
    ///     Gets or sets the Modified Date
    /// </summary>
    [DataMember]
    public DateTime UpdateDate { get; set; }

    /// <summary>
    ///     Gets or sets the Deleted Date
    /// </summary>
    [DataMember]
    public DateTime? DeleteDate { get; set; }

    public virtual void ResetIdentity()
    {
        _hasIdentity = false;
        _id = default;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the current entity has an identity, eg. Id.
    /// </summary>
    public virtual bool HasIdentity
    {
        get => _hasIdentity;
        protected set => _hasIdentity = value;
    }

    /*public virtual bool SameIdentityAs(IEntity other)
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
    }*/

    public object DeepClone() => MemberwiseClone();

    /// <summary>
    ///     Method to call on a property setter.
    /// </summary>
    /// <param name="propertyInfo">The property info.</param>
    protected virtual void OnPropertyChanged(PropertyInfo propertyInfo)
    {
        if (_withChanges == false)
        {
            return;
        }

        _propertyChangedInfo[propertyInfo.Name] = true;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyInfo.Name));
    }

    /// <summary>
    ///     Method to call on entity saved when first added
    /// </summary>
    internal virtual void AddingEntity()
    {
        CreateDate = DateTime.Now;
        UpdateDate = DateTime.Now;
    }

    /// <summary>
    ///     Method to call on entity saved/updated
    /// </summary>
    internal virtual void UpdatingEntity() => UpdateDate = DateTime.Now;

    public static bool operator ==(Item left, Item right) => ReferenceEquals(left, right);

    public static bool operator !=(Item left, Item right) => !(left == right);

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }
}
