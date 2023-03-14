using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an abstract class for base ContentType properties and methods
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
[DebuggerDisplay("Id: {Id}, Name: {Name}, Alias: {Alias}")]
public abstract class ContentTypeBase : TreeEntityBase, IContentTypeBase
{
    // Custom comparer for enumerable
    private static readonly DelegateEqualityComparer<IEnumerable<ContentTypeSort>> ContentTypeSortComparer =
        new(
            (sorts, enumerable) => sorts.UnsortedSequenceEqual(enumerable),
            sorts => sorts.GetHashCode());

    private readonly IShortStringHelper _shortStringHelper;

    private string _alias;
    private bool _allowedAsRoot; // note: only one that's not 'pure element type'
    private IEnumerable<ContentTypeSort>? _allowedContentTypes;
    private string? _description;
    private bool _hasPropertyTypeBeenRemoved;
    private string? _icon = "icon-folder";
    private bool _isContainer;
    private bool _isElement;
    private PropertyGroupCollection _propertyGroups;
    private string? _thumbnail = "folder.png";
    private ContentVariation _variations;

    protected ContentTypeBase(IShortStringHelper shortStringHelper, int parentId)
    {
        _alias = string.Empty;
        _shortStringHelper = shortStringHelper;
        if (parentId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        ParentId = parentId;

        _allowedContentTypes = new List<ContentTypeSort>();
        _propertyGroups = new PropertyGroupCollection();

        // actually OK as IsPublishing is constant
        // ReSharper disable once VirtualMemberCallInConstructor
        PropertyTypeCollection = new PropertyTypeCollection(SupportsPublishing);
        PropertyTypeCollection.CollectionChanged += PropertyTypesChanged;

        _variations = ContentVariation.Nothing;
    }

    protected ContentTypeBase(IShortStringHelper shortStringHelper, IContentTypeBase parent)
        : this(shortStringHelper, parent, string.Empty)
    {
    }

    protected ContentTypeBase(IShortStringHelper shortStringHelper, IContentTypeBase parent, string alias)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        SetParent(parent);

        _shortStringHelper = shortStringHelper;
        _alias = alias;
        _allowedContentTypes = new List<ContentTypeSort>();
        _propertyGroups = new PropertyGroupCollection();

        // actually OK as IsPublishing is constant
        // ReSharper disable once VirtualMemberCallInConstructor
        PropertyTypeCollection = new PropertyTypeCollection(SupportsPublishing);
        PropertyTypeCollection.CollectionChanged += PropertyTypesChanged;

        _variations = ContentVariation.Nothing;
    }

    /// <summary>
    ///     Gets a value indicating whether the content type supports publishing.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A publishing content type supports draft and published values for properties.
    ///         It is possible to retrieve either the draft (default) or published value of a property.
    ///         Setting the value always sets the draft value, which then needs to be published.
    ///     </para>
    ///     <para>
    ///         A non-publishing content type only supports one value for properties. Getting
    ///         the draft or published value of a property returns the same thing, and publishing
    ///         a value property has no effect.
    ///     </para>
    /// </remarks>
    public abstract bool SupportsPublishing { get; }

    /// <summary>
    ///     The Alias of the ContentType
    /// </summary>
    [DataMember]
    public virtual string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(
            value.ToCleanString(_shortStringHelper, CleanStringType.Alias | CleanStringType.UmbracoCase),
            ref _alias!,
            nameof(Alias));
    }

    /// <summary>
    ///     A boolean flag indicating if a property type has been removed from this instance.
    /// </summary>
    /// <remarks>
    ///     This is currently (specifically) used in order to know that we need to refresh the content cache which
    ///     needs to occur when a property has been removed from a content type
    /// </remarks>
    [IgnoreDataMember]
    internal bool HasPropertyTypeBeenRemoved
    {
        get => _hasPropertyTypeBeenRemoved;
        private set
        {
            _hasPropertyTypeBeenRemoved = value;
            OnPropertyChanged(nameof(HasPropertyTypeBeenRemoved));
        }
    }

    /// <summary>
    ///     PropertyTypes that are not part of a PropertyGroup
    /// </summary>
    [IgnoreDataMember]

    // TODO: should we mark this as EditorBrowsable hidden since it really isn't ever used?
    internal PropertyTypeCollection PropertyTypeCollection { get; private set; }

    public abstract ISimpleContentType ToSimple();

    /// <summary>
    ///     Description for the ContentType
    /// </summary>
    [DataMember]
    public string? Description
    {
        get => _description;
        set => SetPropertyValueAndDetectChanges(value, ref _description, nameof(Description));
    }

    /// <summary>
    ///     Name of the icon (sprite class) used to identify the ContentType
    /// </summary>
    [DataMember]
    public string? Icon
    {
        get => _icon;
        set => SetPropertyValueAndDetectChanges(value, ref _icon, nameof(Icon));
    }

    /// <summary>
    ///     Name of the thumbnail used to identify the ContentType
    /// </summary>
    [DataMember]
    public string? Thumbnail
    {
        get => _thumbnail;
        set => SetPropertyValueAndDetectChanges(value, ref _thumbnail, nameof(Thumbnail));
    }

    /// <summary>
    ///     Gets or Sets a boolean indicating whether this ContentType is allowed at the root
    /// </summary>
    [DataMember]
    public bool AllowedAsRoot
    {
        get => _allowedAsRoot;
        set => SetPropertyValueAndDetectChanges(value, ref _allowedAsRoot, nameof(AllowedAsRoot));
    }

    /// <summary>
    ///     Gets or Sets a boolean indicating whether this ContentType is a Container
    /// </summary>
    /// <remarks>
    ///     ContentType Containers doesn't show children in the tree, but rather in grid-type view.
    /// </remarks>
    [DataMember]
    public bool IsContainer
    {
        get => _isContainer;
        set => SetPropertyValueAndDetectChanges(value, ref _isContainer, nameof(IsContainer));
    }

    /// <inheritdoc />
    [DataMember]
    public bool IsElement
    {
        get => _isElement;
        set => SetPropertyValueAndDetectChanges(value, ref _isElement, nameof(IsElement));
    }

    /// <summary>
    ///     Gets or sets a list of integer Ids for allowed ContentTypes
    /// </summary>
    [DataMember]
    public IEnumerable<ContentTypeSort>? AllowedContentTypes
    {
        get => _allowedContentTypes;
        set => SetPropertyValueAndDetectChanges(value, ref _allowedContentTypes, nameof(AllowedContentTypes), ContentTypeSortComparer);
    }

    /// <summary>
    ///     Gets or sets the content variation of the content type.
    /// </summary>
    public virtual ContentVariation Variations
    {
        get => _variations;
        set => SetPropertyValueAndDetectChanges(value, ref _variations, nameof(Variations));
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <para>A PropertyGroup corresponds to a Tab in the UI</para>
    ///     <para>Marked DoNotClone because we will manually deal with cloning and the event handlers</para>
    /// </remarks>
    [DataMember]
    [DoNotClone]
    public PropertyGroupCollection PropertyGroups
    {
        get => _propertyGroups;
        set
        {
            _propertyGroups = value;
            _propertyGroups.CollectionChanged += PropertyGroupsChanged;
            PropertyGroupsChanged(
                _propertyGroups,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <inheritdoc />
    public bool SupportsVariation(string culture, string segment, bool wildcards = false) =>

        // exact validation: cannot accept a 'null' culture if the property type varies
        //  by culture, and likewise for segment
        // wildcard validation: can accept a '*' culture or segment
        Variations.ValidateVariation(culture, segment, true, wildcards, false);

    /// <inheritdoc />
    public bool SupportsPropertyVariation(string culture, string segment, bool wildcards = false) =>

        // non-exact validation: can accept a 'null' culture if the property type varies
        //  by culture, and likewise for segment
        // wildcard validation: can accept a '*' culture or segment
        Variations.ValidateVariation(culture, segment, false, true, false);

    /// <inheritdoc />
    [IgnoreDataMember]
    [DoNotClone]
    public IEnumerable<IPropertyType> PropertyTypes =>
        PropertyTypeCollection.Union(PropertyGroups.SelectMany(x => x.PropertyTypes!));

    /// <inheritdoc />
    [DoNotClone]
    public IEnumerable<IPropertyType> NoGroupPropertyTypes
    {
        get => PropertyTypeCollection;
        set
        {
            if (PropertyTypeCollection != null)
            {
                PropertyTypeCollection.ClearCollectionChangedEvents();
            }

            PropertyTypeCollection = new PropertyTypeCollection(SupportsPublishing, value);
            PropertyTypeCollection.CollectionChanged += PropertyTypesChanged;
            PropertyTypesChanged(
                PropertyTypeCollection,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    /// <summary>
    ///     Checks whether a PropertyType with a given alias already exists
    /// </summary>
    /// <param name="alias">Alias of the PropertyType</param>
    /// <returns>Returns <c>True</c> if a PropertyType with the passed in alias exists, otherwise <c>False</c></returns>
    public abstract bool PropertyTypeExists(string? alias);

    /// <inheritdoc />
    public abstract bool AddPropertyGroup(string alias, string name);

    /// <inheritdoc />
    public abstract bool AddPropertyType(IPropertyType propertyType, string propertyGroupAlias, string? propertyGroupName = null);

    /// <summary>
    ///     Adds a PropertyType, which does not belong to a PropertyGroup.
    /// </summary>
    /// <param name="propertyType"><see cref="IPropertyType" /> to add</param>
    /// <returns>Returns <c>True</c> if PropertyType was added, otherwise <c>False</c></returns>
    public bool AddPropertyType(IPropertyType propertyType)
    {
        if (PropertyTypeExists(propertyType.Alias) == false)
        {
            PropertyTypeCollection.Add(propertyType);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Moves a PropertyType to a specified PropertyGroup
    /// </summary>
    /// <param name="propertyTypeAlias">Alias of the PropertyType to move</param>
    /// <param name="propertyGroupAlias">Alias of the PropertyGroup to move the PropertyType to</param>
    /// <returns></returns>
    /// <remarks>
    ///     If <paramref name="propertyGroupAlias" /> is null then the property is moved back to
    ///     "generic properties" ie does not have a tab anymore.
    /// </remarks>
    public bool MovePropertyType(string propertyTypeAlias, string propertyGroupAlias)
    {
        // get property, ensure it exists
        IPropertyType? propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        if (propertyType == null)
        {
            return false;
        }

        // get new group, if required, and ensure it exists
        PropertyGroup? newPropertyGroup = null;
        if (propertyGroupAlias != null)
        {
            var index = PropertyGroups.IndexOfKey(propertyGroupAlias);
            if (index == -1)
            {
                return false;
            }

            newPropertyGroup = PropertyGroups[index];
        }

        // get old group
        PropertyGroup? oldPropertyGroup = PropertyGroups.FirstOrDefault(x => x.PropertyTypes?.Any(y => y.Alias == propertyTypeAlias) ?? false);

        // set new group
        propertyType.PropertyGroupId =
            newPropertyGroup == null ? null : new Lazy<int>(() => newPropertyGroup.Id, false);

        // remove from old group, if any - add to new group, if any
        oldPropertyGroup?.PropertyTypes?.RemoveItem(propertyTypeAlias);
        newPropertyGroup?.PropertyTypes?.Add(propertyType);

        return true;
    }

    /// <summary>
    ///     Removes a PropertyType from the current ContentType
    /// </summary>
    /// <param name="alias">Alias of the <see cref="IPropertyType" /> to remove</param>
    public void RemovePropertyType(string alias)
    {
        // check through each property group to see if we can remove the property type by alias from it
        foreach (PropertyGroup propertyGroup in PropertyGroups)
        {
            if (propertyGroup.PropertyTypes?.RemoveItem(alias) ?? false)
            {
                if (!HasPropertyTypeBeenRemoved)
                {
                    HasPropertyTypeBeenRemoved = true;
                    OnPropertyChanged(nameof(PropertyTypes));
                }

                break;
            }
        }

        // check through each local property type collection (not assigned to a tab)
        if (PropertyTypeCollection.RemoveItem(alias))
        {
            if (!HasPropertyTypeBeenRemoved)
            {
                HasPropertyTypeBeenRemoved = true;
                OnPropertyChanged(nameof(PropertyTypes));
            }
        }
    }

    /// <summary>
    ///     Removes a PropertyGroup from the current ContentType
    /// </summary>
    /// <param name="alias">Alias of the <see cref="PropertyGroup" /> to remove</param>
    public void RemovePropertyGroup(string alias)
    {
        // if no group exists with that alias, do nothing
        var index = PropertyGroups.IndexOfKey(alias);
        if (index == -1)
        {
            return;
        }

        PropertyGroup group = PropertyGroups[index];

        // first remove the group
        PropertyGroups.Remove(group);

        if (group.PropertyTypes is not null)
        {
            // Then re-assign the group's properties to no group
            foreach (IPropertyType property in group.PropertyTypes)
            {
                property.PropertyGroupId = null;
                PropertyTypeCollection.Add(property);
            }
        }

        OnPropertyChanged(nameof(PropertyGroups));
    }

    /// <summary>
    ///     Indicates whether the current entity is dirty.
    /// </summary>
    /// <returns>True if entity is dirty, otherwise False</returns>
    public override bool IsDirty()
    {
        var dirtyEntity = base.IsDirty();

        var dirtyGroups = PropertyGroups.Any(x => x.IsDirty());
        var dirtyTypes = PropertyTypes.Any(x => x.IsDirty());

        return dirtyEntity || dirtyGroups || dirtyTypes;
    }

    /// <summary>
    ///     Resets dirty properties by clearing the dictionary used to track changes.
    /// </summary>
    /// <remarks>
    ///     Please note that resetting the dirty properties could potentially
    ///     obstruct the saving of a new or updated entity.
    /// </remarks>
    public override void ResetDirtyProperties()
    {
        base.ResetDirtyProperties();

        // loop through each property group to reset the property types
        var propertiesReset = new List<int>();

        foreach (PropertyGroup propertyGroup in PropertyGroups)
        {
            propertyGroup.ResetDirtyProperties();
            if (propertyGroup.PropertyTypes is not null)
            {
                foreach (IPropertyType propertyType in propertyGroup.PropertyTypes)
                {
                    propertyType.ResetDirtyProperties();
                    propertiesReset.Add(propertyType.Id);
                }
            }
        }

        // then loop through our property type collection since some might not exist on a property group
        // but don't re-reset ones we've already done.
        foreach (IPropertyType propertyType in PropertyTypes.Where(x => propertiesReset.Contains(x.Id) == false))
        {
            propertyType.ResetDirtyProperties();
        }
    }

    public ContentTypeBase DeepCloneWithResetIdentities(string alias)
    {
        var clone = (ContentTypeBase)DeepClone();
        clone.Alias = alias;
        clone.Key = Guid.Empty;
        foreach (PropertyGroup propertyGroup in clone.PropertyGroups)
        {
            propertyGroup.ResetIdentity();
            propertyGroup.ResetDirtyProperties(false);
        }

        foreach (IPropertyType propertyType in clone.PropertyTypes)
        {
            propertyType.ResetIdentity();
            propertyType.ResetDirtyProperties(false);
        }

        clone.ResetIdentity();
        clone.ResetDirtyProperties(false);
        return clone;
    }

    protected void PropertyGroupsChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(PropertyGroups));

    protected void PropertyTypesChanged(object? sender, NotifyCollectionChangedEventArgs e) =>

        // enable this to detect duplicate property aliases. We do want this, however making this change in a
        // patch release might be a little dangerous
        ////detect if there are any duplicate aliases - this cannot be allowed
        // if (e.Action == NotifyCollectionChangedAction.Add
        //    || e.Action == NotifyCollectionChangedAction.Replace)
        // {
        //    var allAliases = _noGroupPropertyTypes.Concat(PropertyGroups.SelectMany(x => x.PropertyTypes)).Select(x => x.Alias);
        //    if (allAliases.HasDuplicates(false))
        //    {
        //        var newAliases = string.Join(", ", e.NewItems.Cast<PropertyType>().Select(x => x.Alias));
        //        throw new InvalidOperationException($"Other property types already exist with the aliases: {newAliases}");
        //    }
        // }
        OnPropertyChanged(nameof(PropertyTypes));

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (ContentTypeBase)clone;

        if (clonedEntity.PropertyTypeCollection != null)
        {
            // need to manually wire up the event handlers for the property type collections - we've ensured
            // its ignored from the auto-clone process because its return values are unions, not raw and
            // we end up with duplicates, see: http://issues.umbraco.org/issue/U4-4842
            clonedEntity.PropertyTypeCollection.ClearCollectionChangedEvents(); // clear this event handler if any
            clonedEntity.PropertyTypeCollection =
                (PropertyTypeCollection)PropertyTypeCollection.DeepClone(); // manually deep clone
            clonedEntity.PropertyTypeCollection.CollectionChanged +=
                clonedEntity.PropertyTypesChanged; // re-assign correct event handler
        }

        if (clonedEntity._propertyGroups != null)
        {
            clonedEntity._propertyGroups.ClearCollectionChangedEvents(); // clear this event handler if any
            clonedEntity._propertyGroups = (PropertyGroupCollection)_propertyGroups.DeepClone(); // manually deep clone
            clonedEntity._propertyGroups.CollectionChanged +=
                clonedEntity.PropertyGroupsChanged; // re-assign correct event handler
        }
    }
}
