using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a group of property types.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
[DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
public class PropertyGroup : EntityBase, IEquatable<PropertyGroup>
{
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This field is for internal use only (to allow changing item keys).")]
    internal PropertyGroupCollection? Collection;

    private string _alias;
    private string? _name;
    private PropertyTypeCollection? _propertyTypes;
    private int _sortOrder;

    private PropertyGroupType _type;

    public PropertyGroup(bool isPublishing)
        : this(new PropertyTypeCollection(isPublishing))
    {
    }

    public PropertyGroup(PropertyTypeCollection propertyTypeCollection)
    {
        PropertyTypes = propertyTypeCollection;
        _alias = string.Empty;
    }

    /// <summary>
    ///     Gets or sets the type of the group.
    /// </summary>
    /// <value>
    ///     The type.
    /// </value>
    [DataMember]
    public PropertyGroupType Type
    {
        get => _type;
        set => SetPropertyValueAndDetectChanges(value, ref _type, nameof(Type));
    }

    /// <summary>
    ///     Gets or sets the name of the group.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets the alias of the group.
    /// </summary>
    /// <value>
    ///     The alias.
    /// </value>
    [DataMember]
    public string Alias
    {
        get => _alias;
        set
        {
            // If added to a collection, ensure the key is changed before setting it (this ensures the internal lookup dictionary is updated)
            Collection?.ChangeKey(this, value);

            SetPropertyValueAndDetectChanges(value, ref _alias!, nameof(Alias));
        }
    }

    /// <summary>
    ///     Gets or sets the sort order of the group.
    /// </summary>
    /// <value>
    ///     The sort order.
    /// </value>
    [DataMember]
    public int SortOrder
    {
        get => _sortOrder;
        set => SetPropertyValueAndDetectChanges(value, ref _sortOrder, nameof(SortOrder));
    }

    /// <summary>
    ///     Gets or sets a collection of property types for the group.
    /// </summary>
    /// <value>
    ///     The property types.
    /// </value>
    /// <remarks>
    ///     Marked with DoNotClone, because we will manually deal with cloning and the event handlers.
    /// </remarks>
    [DataMember]
    [DoNotClone]
    public PropertyTypeCollection? PropertyTypes
    {
        get => _propertyTypes;
        set
        {
            if (_propertyTypes != null)
            {
                _propertyTypes.ClearCollectionChangedEvents();
            }

            _propertyTypes = value;

            if (_propertyTypes is not null)
            {
                // since we're adding this collection to this group,
                // we need to ensure that all the lazy values are set.
                foreach (IPropertyType propertyType in _propertyTypes)
                {
                    propertyType.PropertyGroupId = new Lazy<int>(() => Id);
                }

                OnPropertyChanged(nameof(PropertyTypes));
                _propertyTypes.CollectionChanged += PropertyTypesChanged;
            }
        }
    }

    public bool Equals(PropertyGroup? other) =>
        base.Equals(other) || (other != null && Type == other.Type && Alias == other.Alias);

    public override int GetHashCode() => (base.GetHashCode(), Type, Alias).GetHashCode();

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (PropertyGroup)clone;
        clonedEntity.Collection = null;

        if (clonedEntity._propertyTypes != null)
        {
            clonedEntity._propertyTypes.ClearCollectionChangedEvents(); // clear this event handler if any
            clonedEntity._propertyTypes = (PropertyTypeCollection)_propertyTypes!.DeepClone(); // manually deep clone
            clonedEntity._propertyTypes.CollectionChanged +=
                clonedEntity.PropertyTypesChanged; // re-assign correct event handler
        }
    }

    private void PropertyTypesChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(PropertyTypes));
}
