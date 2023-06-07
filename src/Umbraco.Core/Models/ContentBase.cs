using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents an abstract class for base Content properties and methods
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
[DebuggerDisplay("Id: {Id}, Name: {Name}, ContentType: {ContentType.Alias}")]
public abstract class ContentBase : TreeEntityBase, IContentBase
{
    private int _contentTypeId;
    private ContentCultureInfosCollection? _cultureInfos;
    private IPropertyCollection _properties;
    private int _writerId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentBase" /> class.
    /// </summary>
    protected ContentBase(string? name, int parentId, IContentTypeComposition? contentType, IPropertyCollection properties, string? culture = null)
        : this(name, contentType, properties, culture)
    {
        if (parentId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(parentId));
        }

        ParentId = parentId;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentBase" /> class.
    /// </summary>
    protected ContentBase(string? name, IContentBase? parent, IContentTypeComposition contentType, IPropertyCollection properties, string? culture = null)
        : this(name, contentType, properties, culture)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        SetParent(parent);
    }

    private ContentBase(string? name, IContentTypeComposition? contentType, IPropertyCollection properties, string? culture = null)
    {
        ContentType = contentType?.ToSimple() ?? throw new ArgumentNullException(nameof(contentType));

        // initially, all new instances have
        Id = 0; // no identity
        VersionId = 0; // no versions

        SetCultureName(name, culture);

        _contentTypeId = contentType.Id;
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        _properties.EnsurePropertyTypes(contentType.CompositionPropertyTypes);
    }

    [IgnoreDataMember]
    public ISimpleContentType ContentType { get; private set; }

    /// <summary>
    ///     Id of the user who wrote/updated this entity
    /// </summary>
    [DataMember]
    public int WriterId
    {
        get => _writerId;
        set => SetPropertyValueAndDetectChanges(value, ref _writerId, nameof(WriterId));
    }

    [IgnoreDataMember]
    public int VersionId { get; set; }

    /// <summary>
    ///     Integer Id of the default ContentType
    /// </summary>
    [DataMember]
    public int ContentTypeId
    {
        get
        {
            // There will be cases where this has not been updated to reflect the true content type ID.
            // This will occur when inserting new content.
            if (_contentTypeId == 0 && ContentType != null)
            {
                _contentTypeId = ContentType.Id;
            }

            return _contentTypeId;
        }
        private set => SetPropertyValueAndDetectChanges(value, ref _contentTypeId, nameof(ContentTypeId));
    }

    /// <summary>
    ///     Gets or sets the collection of properties for the entity.
    /// </summary>
    /// <remarks>
    ///     Marked DoNotClone since we'll manually clone the underlying field to deal with the event handling
    /// </remarks>
    [DataMember]
    [DoNotClone]
    public IPropertyCollection Properties
    {
        get => _properties;
        set
        {
            if (_properties != null)
            {
                _properties.ClearCollectionChangedEvents();
            }

            _properties = value;
            _properties.CollectionChanged += PropertiesChanged;
        }
    }

    public void ChangeContentType(ISimpleContentType contentType)
    {
        ContentType = contentType;
        ContentTypeId = contentType.Id;
    }

    protected void PropertiesChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        OnPropertyChanged(nameof(Properties));

    /// <inheritdoc />
    /// <remarks>
    ///     Overridden to deal with specific object instances
    /// </remarks>
    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedContent = (ContentBase)clone;
        // Need to manually clone this since it's not settable
        clonedContent.ContentType = ContentType;

        // If culture infos exist then deal with event bindings
        if (clonedContent._cultureInfos != null)
        {
            clonedContent._cultureInfos.ClearCollectionChangedEvents(); // clear this event handler if any
            clonedContent._cultureInfos =
                (ContentCultureInfosCollection?)_cultureInfos?.DeepClone(); // manually deep clone
            if (clonedContent._cultureInfos is not null)
            {
                clonedContent._cultureInfos.CollectionChanged +=
                    clonedContent.CultureInfosCollectionChanged; // re-assign correct event handler
            }
        }

        // if properties exist then deal with event bindings
        if (clonedContent._properties != null)
        {
            clonedContent._properties.ClearCollectionChangedEvents(); // clear this event handler if any
            clonedContent._properties = (IPropertyCollection)_properties.DeepClone(); // manually deep clone
            clonedContent._properties.CollectionChanged +=
                clonedContent.PropertiesChanged; // re-assign correct event handler
        }

        clonedContent._currentCultureChanges.updatedCultures = null;
        clonedContent._currentCultureChanges.addedCultures = null;
        clonedContent._currentCultureChanges.removedCultures = null;

        clonedContent._previousCultureChanges.updatedCultures = null;
        clonedContent._previousCultureChanges.addedCultures = null;
        clonedContent._previousCultureChanges.removedCultures = null;
    }

    #region Used for change tracking

    private (HashSet<string>? addedCultures, HashSet<string>? removedCultures, HashSet<string>? updatedCultures)
        _currentCultureChanges;

    private (HashSet<string>? addedCultures, HashSet<string>? removedCultures, HashSet<string>? updatedCultures)
        _previousCultureChanges;

    public static class ChangeTrackingPrefix
    {
        public const string UpdatedCulture = "_updatedCulture_";
        public const string ChangedCulture = "_changedCulture_";
        public const string PublishedCulture = "_publishedCulture_";
        public const string UnpublishedCulture = "_unpublishedCulture_";
        public const string AddedCulture = "_addedCulture_";
        public const string RemovedCulture = "_removedCulture_";
    }

    #endregion

    #region Cultures

    // notes - common rules
    // - setting a variant value on an invariant content type throws
    // - getting a variant value on an invariant content type returns null
    // - setting and getting the invariant value is always possible
    // - setting a null value clears the value

    /// <inheritdoc />
    public IEnumerable<string> AvailableCultures
        => _cultureInfos?.Keys ?? Enumerable.Empty<string>();

    /// <inheritdoc />
    public bool IsCultureAvailable(string culture)
        => _cultureInfos != null && _cultureInfos.ContainsKey(culture);

    /// <inheritdoc />
    [DataMember]
    public ContentCultureInfosCollection? CultureInfos
    {
        get
        {
            if (_cultureInfos != null)
            {
                return _cultureInfos;
            }

            _cultureInfos = new ContentCultureInfosCollection();
            _cultureInfos.CollectionChanged += CultureInfosCollectionChanged;
            return _cultureInfos;
        }

        set
        {
            if (_cultureInfos != null)
            {
                _cultureInfos.ClearCollectionChangedEvents();
            }

            _cultureInfos = value;
            if (_cultureInfos != null)
            {
                _cultureInfos.CollectionChanged += CultureInfosCollectionChanged;
            }
        }
    }

    /// <inheritdoc />
    public string? GetCultureName(string? culture)
    {
        if (culture.IsNullOrWhiteSpace())
        {
            return Name;
        }

        if (!ContentType.VariesByCulture())
        {
            return null;
        }

        if (_cultureInfos == null)
        {
            return null;
        }

        return _cultureInfos.TryGetValue(culture!, out ContentCultureInfos infos) ? infos.Name : null;
    }

    /// <inheritdoc />
    public DateTime? GetUpdateDate(string culture)
    {
        if (culture.IsNullOrWhiteSpace())
        {
            return null;
        }

        if (!ContentType.VariesByCulture())
        {
            return null;
        }

        if (_cultureInfos == null)
        {
            return null;
        }

        return _cultureInfos.TryGetValue(culture, out ContentCultureInfos infos) ? infos.Date : null;
    }

    /// <inheritdoc />
    public void SetCultureName(string? name, string? culture)
    {
        // set on variant content type
        if (ContentType.VariesByCulture())
        {
            // invariant is ok
            if (culture.IsNullOrWhiteSpace())
            {
                Name = name; // may be null
            }

            // clear
            else if (name.IsNullOrWhiteSpace())
            {
                ClearCultureInfo(culture!);
            }

            // set
            else
            {
                this.SetCultureInfo(culture!, name, DateTime.Now);
            }
        }

        // set on invariant content type
        else
        {
            // invariant is NOT ok
            if (!culture.IsNullOrWhiteSpace())
            {
                throw new NotSupportedException("Content type does not vary by culture.");
            }

            Name = name; // may be null
        }
    }

    private void ClearCultureInfo(string culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        if (string.IsNullOrWhiteSpace(culture))
        {
            throw new ArgumentException("Value can't be empty or consist only of white-space characters.", nameof(culture));
        }

        if (_cultureInfos == null)
        {
            return;
        }

        _cultureInfos.Remove(culture);
        if (_cultureInfos.Count == 0)
        {
            _cultureInfos = null;
        }
    }

    /// <summary>
    ///     Handles culture infos collection changes.
    /// </summary>
    private void CultureInfosCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CultureInfos));

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                ContentCultureInfos? cultureInfo = e.NewItems?.Cast<ContentCultureInfos>().First();
                if (_currentCultureChanges.addedCultures == null)
                {
                    _currentCultureChanges.addedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (_currentCultureChanges.updatedCultures == null)
                {
                    _currentCultureChanges.updatedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentCultureChanges.addedCultures.Add(cultureInfo.Culture);
                    _currentCultureChanges.updatedCultures.Add(cultureInfo.Culture);
                    _currentCultureChanges.removedCultures?.Remove(cultureInfo.Culture);
                }

                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                // Remove listening for changes
                ContentCultureInfos? cultureInfo = e.OldItems?.Cast<ContentCultureInfos>().First();
                if (_currentCultureChanges.removedCultures == null)
                {
                    _currentCultureChanges.removedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentCultureChanges.removedCultures.Add(cultureInfo.Culture);
                    _currentCultureChanges.updatedCultures?.Remove(cultureInfo.Culture);
                    _currentCultureChanges.addedCultures?.Remove(cultureInfo.Culture);
                }

                break;
            }

            case NotifyCollectionChangedAction.Replace:
            {
                // Replace occurs when an Update occurs
                ContentCultureInfos? cultureInfo = e.NewItems?.Cast<ContentCultureInfos>().First();
                if (_currentCultureChanges.updatedCultures == null)
                {
                    _currentCultureChanges.updatedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentCultureChanges.updatedCultures.Add(cultureInfo.Culture);
                }

                break;
            }
        }
    }

    #endregion

    #region Has, Get, Set, Publish Property Value

    /// <inheritdoc />
    public bool HasProperty(string propertyTypeAlias)
        => Properties.Contains(propertyTypeAlias);

    /// <inheritdoc />
    public object? GetValue(string propertyTypeAlias, string? culture = null, string? segment = null, bool published = false) =>
        Properties.TryGetValue(propertyTypeAlias, out IProperty? property)
            ? property?.GetValue(culture, segment, published)
            : null;

    /// <inheritdoc />
    public TValue? GetValue<TValue>(string propertyTypeAlias, string? culture = null, string? segment = null, bool published = false)
    {
        if (!Properties.TryGetValue(propertyTypeAlias, out IProperty? property))
        {
            return default;
        }

        Attempt<TValue>? convertAttempt = property?.GetValue(culture, segment, published).TryConvertTo<TValue>();
        return convertAttempt?.Success is not null && (convertAttempt?.Success ?? false)
            ? convertAttempt.Value.Result
            : default;
    }

    /// <inheritdoc />
    public void SetValue(string propertyTypeAlias, object? value, string? culture = null, string? segment = null)
    {
        if (!Properties.TryGetValue(propertyTypeAlias, out IProperty? property))
        {
            throw new InvalidOperationException(
                $"No PropertyType exists with the supplied alias \"{propertyTypeAlias}\".");
        }

        property?.SetValue(value, culture, segment);

        // bump the culture to be flagged for updating
        this.TouchCulture(culture);
    }

    #endregion

    #region Dirty

    public override void ResetWereDirtyProperties()
    {
        base.ResetWereDirtyProperties();
        _previousCultureChanges.addedCultures = null;
        _previousCultureChanges.removedCultures = null;
        _previousCultureChanges.updatedCultures = null;
    }

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override void ResetDirtyProperties(bool rememberDirty)
    {
        base.ResetDirtyProperties(rememberDirty);

        if (rememberDirty)
        {
            _previousCultureChanges.addedCultures =
                _currentCultureChanges.addedCultures == null || _currentCultureChanges.addedCultures.Count == 0
                    ? null
                    : new HashSet<string>(
                        _currentCultureChanges.addedCultures,
                        StringComparer.InvariantCultureIgnoreCase);
            _previousCultureChanges.removedCultures =
                _currentCultureChanges.removedCultures == null || _currentCultureChanges.removedCultures.Count == 0
                    ? null
                    : new HashSet<string>(
                        _currentCultureChanges.removedCultures,
                        StringComparer.InvariantCultureIgnoreCase);
            _previousCultureChanges.updatedCultures =
                _currentCultureChanges.updatedCultures == null || _currentCultureChanges.updatedCultures.Count == 0
                    ? null
                    : new HashSet<string>(
                        _currentCultureChanges.updatedCultures,
                        StringComparer.InvariantCultureIgnoreCase);
        }
        else
        {
            _previousCultureChanges.addedCultures = null;
            _previousCultureChanges.removedCultures = null;
            _previousCultureChanges.updatedCultures = null;
        }

        _currentCultureChanges.addedCultures?.Clear();
        _currentCultureChanges.removedCultures?.Clear();
        _currentCultureChanges.updatedCultures?.Clear();

        // also reset dirty changes made to user's properties
        foreach (IProperty prop in Properties)
        {
            prop.ResetDirtyProperties(rememberDirty);
        }

        // take care of culture infos
        if (_cultureInfos == null)
        {
            return;
        }

        foreach (ContentCultureInfos cultureInfo in _cultureInfos)
        {
            cultureInfo.ResetDirtyProperties(rememberDirty);
        }
    }

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override bool IsDirty() => IsEntityDirty() || this.IsAnyUserPropertyDirty();

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override bool WasDirty() => WasEntityDirty() || this.WasAnyUserPropertyDirty();

    /// <summary>
    ///     Gets a value indicating whether the current entity's own properties (not user) are dirty.
    /// </summary>
    public bool IsEntityDirty() => base.IsDirty();

    /// <summary>
    ///     Gets a value indicating whether the current entity's own properties (not user) were dirty.
    /// </summary>
    public bool WasEntityDirty() => base.WasDirty();

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override bool IsPropertyDirty(string propertyName)
    {
        if (base.IsPropertyDirty(propertyName))
        {
            return true;
        }

        // Special check here since we want to check if the request is for changed cultures
        if (propertyName.StartsWith(ChangeTrackingPrefix.AddedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.AddedCulture);
            return _currentCultureChanges.addedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.RemovedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.RemovedCulture);
            return _currentCultureChanges.removedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.UpdatedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.UpdatedCulture);
            return _currentCultureChanges.updatedCultures?.Contains(culture) ?? false;
        }

        return Properties.Contains(propertyName) && (Properties[propertyName]?.IsDirty() ?? false);
    }

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override bool WasPropertyDirty(string propertyName)
    {
        if (base.WasPropertyDirty(propertyName))
        {
            return true;
        }

        // Special check here since we want to check if the request is for changed cultures
        if (propertyName.StartsWith(ChangeTrackingPrefix.AddedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.AddedCulture);
            return _previousCultureChanges.addedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.RemovedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.RemovedCulture);
            return _previousCultureChanges.removedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.UpdatedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.UpdatedCulture);
            return _previousCultureChanges.updatedCultures?.Contains(culture) ?? false;
        }

        return Properties.Contains(propertyName) && (Properties[propertyName]?.WasDirty() ?? false);
    }

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override IEnumerable<string> GetDirtyProperties()
    {
        IEnumerable<string> instanceProperties = base.GetDirtyProperties();
        IEnumerable<string> propertyTypes = Properties.Where(x => x.IsDirty()).Select(x => x.Alias);
        return instanceProperties.Concat(propertyTypes);
    }

    /// <inheritdoc />
    /// <remarks>Overridden to include user properties.</remarks>
    public override IEnumerable<string> GetWereDirtyProperties()
    {
        IEnumerable<string> instanceProperties = base.GetWereDirtyProperties();
        IEnumerable<string> propertyTypes = Properties.Where(x => x.WasDirty()).Select(x => x.Alias);
        return instanceProperties.Concat(propertyTypes);
    }

    #endregion
}
