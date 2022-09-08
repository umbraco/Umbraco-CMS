using System.Collections.Specialized;
using System.Runtime.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Content object
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Content : ContentBase, IContent
{
    private HashSet<string>? _editedCultures;
    private bool _published;
    private PublishedState _publishedState;
    private ContentCultureInfosCollection? _publishInfos;
    private int? _templateId;

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, string? culture = null)
        : this(name, parent, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="userId">The identifier of the user creating the Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, int userId, string? culture = null)
        : this(name, parent, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parent">Parent <see cref="IContent" /> object</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, IContent parent, IContentType contentType, PropertyCollection properties, string? culture = null)
        : base(name, parent, contentType, properties, culture)
    {
        if (contentType == null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        _publishedState = PublishedState.Unpublished;
        PublishedVersionId = 0;
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string? name, int parentId, IContentType? contentType, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="userId">The identifier of the user creating the Content object</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string name, int parentId, IContentType contentType, int userId, string? culture = null)
        : this(name, parentId, contentType, new PropertyCollection(), culture)
    {
        CreatorId = userId;
        WriterId = userId;
    }

    /// <summary>
    ///     Constructor for creating a Content object
    /// </summary>
    /// <param name="name">Name of the content</param>
    /// <param name="parentId">Id of the Parent content</param>
    /// <param name="contentType">ContentType for the current Content object</param>
    /// <param name="properties">Collection of properties</param>
    /// <param name="culture">An optional culture.</param>
    public Content(string? name, int parentId, IContentType? contentType, PropertyCollection properties, string? culture = null)
        : base(name, parentId, contentType, properties, culture)
    {
        if (contentType == null)
        {
            throw new ArgumentNullException(nameof(contentType));
        }

        _publishedState = PublishedState.Unpublished;
        PublishedVersionId = 0;
    }

    /// <summary>
    ///     Gets or sets the template used by the Content.
    ///     This is used to override the default one from the ContentType.
    /// </summary>
    /// <remarks>
    ///     If no template is explicitly set on the Content object,
    ///     the Default template from the ContentType will be returned.
    /// </remarks>
    [DataMember]
    public int? TemplateId
    {
        get => _templateId;
        set => SetPropertyValueAndDetectChanges(value, ref _templateId, nameof(TemplateId));
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this content item is published or not.
    /// </summary>
    /// <remarks>
    ///     the setter is should only be invoked from
    ///     - the ContentFactory when creating a content entity from a dto
    ///     - the ContentRepository when updating a content entity
    /// </remarks>
    [DataMember]
    public bool Published
    {
        get => _published;
        set
        {
            SetPropertyValueAndDetectChanges(value, ref _published, nameof(Published));
            _publishedState = _published ? PublishedState.Published : PublishedState.Unpublished;
        }
    }

    /// <summary>
    ///     Gets the published state of the content item.
    /// </summary>
    /// <remarks>
    ///     The state should be Published or Unpublished, depending on whether Published
    ///     is true or false, but can also temporarily be Publishing or Unpublishing when the
    ///     content item is about to be saved.
    /// </remarks>
    [DataMember]
    public PublishedState PublishedState
    {
        get => _publishedState;
        set
        {
            if (value != PublishedState.Publishing && value != PublishedState.Unpublishing)
            {
                throw new ArgumentException("Invalid state, only Publishing and Unpublishing are accepted.");
            }

            _publishedState = value;
        }
    }

    [IgnoreDataMember]
    public bool Edited { get; set; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public DateTime? PublishDate { get; set; } // set by persistence

    /// <inheritdoc />
    [IgnoreDataMember]
    public int? PublisherId { get; set; } // set by persistence

    /// <inheritdoc />
    [IgnoreDataMember]
    public int? PublishTemplateId { get; set; } // set by persistence

    /// <inheritdoc />
    [IgnoreDataMember]
    public string? PublishName { get; set; } // set by persistence

    /// <inheritdoc />
    [IgnoreDataMember]
    public IEnumerable<string>? EditedCultures
    {
        get => CultureInfos?.Keys.Where(IsCultureEdited);
        set => _editedCultures = value == null ? null : new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    [IgnoreDataMember]
    public IEnumerable<string> PublishedCultures => _publishInfos?.Keys ?? Enumerable.Empty<string>();

    /// <inheritdoc />
    public bool IsCulturePublished(string culture)

        // just check _publishInfos
        // a non-available culture could not become published anyways
        => !culture.IsNullOrWhiteSpace() && _publishInfos != null && _publishInfos.ContainsKey(culture);

    /// <inheritdoc />
    public bool IsCultureEdited(string culture)
        => IsCultureAvailable(culture) && // is available, and
           (!IsCulturePublished(culture) || // is not published, or
            (_editedCultures != null && _editedCultures.Contains(culture))); // is edited

    /// <inheritdoc />
    [IgnoreDataMember]
    public ContentCultureInfosCollection? PublishCultureInfos
    {
        get
        {
            if (_publishInfos != null)
            {
                return _publishInfos;
            }

            _publishInfos = new ContentCultureInfosCollection();
            _publishInfos.CollectionChanged += PublishNamesCollectionChanged;
            return _publishInfos;
        }

        set
        {
            if (_publishInfos != null)
            {
                _publishInfos.ClearCollectionChangedEvents();
            }

            _publishInfos = value;
            if (_publishInfos != null)
            {
                _publishInfos.CollectionChanged += PublishNamesCollectionChanged;
            }
        }
    }

    /// <inheritdoc />
    public string? GetPublishName(string? culture)
    {
        if (culture.IsNullOrWhiteSpace())
        {
            return PublishName;
        }

        if (!ContentType.VariesByCulture())
        {
            return null;
        }

        if (_publishInfos == null)
        {
            return null;
        }

        return _publishInfos.TryGetValue(culture!, out ContentCultureInfos infos) ? infos.Name : null;
    }

    /// <inheritdoc />
    public DateTime? GetPublishDate(string culture)
    {
        if (culture.IsNullOrWhiteSpace())
        {
            return PublishDate;
        }

        if (!ContentType.VariesByCulture())
        {
            return null;
        }

        if (_publishInfos == null)
        {
            return null;
        }

        return _publishInfos.TryGetValue(culture, out ContentCultureInfos infos) ? infos.Date : null;
    }

    [IgnoreDataMember]
    public int PublishedVersionId { get; set; }

    [DataMember]
    public bool Blueprint { get; set; }

    public override void ResetWereDirtyProperties()
    {
        base.ResetWereDirtyProperties();
        _previousPublishCultureChanges.updatedCultures = null;
        _previousPublishCultureChanges.removedCultures = null;
        _previousPublishCultureChanges.addedCultures = null;
    }

    public override void ResetDirtyProperties(bool rememberDirty)
    {
        base.ResetDirtyProperties(rememberDirty);

        if (rememberDirty)
        {
            _previousPublishCultureChanges.addedCultures =
                _currentPublishCultureChanges.addedCultures == null ||
                _currentPublishCultureChanges.addedCultures.Count == 0
                    ? null
                    : new HashSet<string>(_currentPublishCultureChanges.addedCultures, StringComparer.InvariantCultureIgnoreCase);
            _previousPublishCultureChanges.removedCultures =
                _currentPublishCultureChanges.removedCultures == null ||
                _currentPublishCultureChanges.removedCultures.Count == 0
                    ? null
                    : new HashSet<string>(_currentPublishCultureChanges.removedCultures, StringComparer.InvariantCultureIgnoreCase);
            _previousPublishCultureChanges.updatedCultures =
                _currentPublishCultureChanges.updatedCultures == null ||
                _currentPublishCultureChanges.updatedCultures.Count == 0
                    ? null
                    : new HashSet<string>(_currentPublishCultureChanges.updatedCultures, StringComparer.InvariantCultureIgnoreCase);
        }
        else
        {
            _previousPublishCultureChanges.addedCultures = null;
            _previousPublishCultureChanges.removedCultures = null;
            _previousPublishCultureChanges.updatedCultures = null;
        }

        _currentPublishCultureChanges.addedCultures?.Clear();
        _currentPublishCultureChanges.removedCultures?.Clear();
        _currentPublishCultureChanges.updatedCultures?.Clear();

        // take care of the published state
        _publishedState = _published ? PublishedState.Published : PublishedState.Unpublished;

        if (_publishInfos == null)
        {
            return;
        }

        foreach (ContentCultureInfos infos in _publishInfos)
        {
            infos.ResetDirtyProperties(rememberDirty);
        }
    }

    /// <inheritdoc />
    /// <remarks>Overridden to check special keys.</remarks>
    public override bool IsPropertyDirty(string propertyName)
    {
        // Special check here since we want to check if the request is for changed cultures
        if (propertyName.StartsWith(ChangeTrackingPrefix.PublishedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.PublishedCulture);
            return _currentPublishCultureChanges.addedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.UnpublishedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.UnpublishedCulture);
            return _currentPublishCultureChanges.removedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.ChangedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.ChangedCulture);
            return _currentPublishCultureChanges.updatedCultures?.Contains(culture) ?? false;
        }

        return base.IsPropertyDirty(propertyName);
    }

    /// <inheritdoc />
    /// <remarks>Overridden to check special keys.</remarks>
    public override bool WasPropertyDirty(string propertyName)
    {
        // Special check here since we want to check if the request is for changed cultures
        if (propertyName.StartsWith(ChangeTrackingPrefix.PublishedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.PublishedCulture);
            return _previousPublishCultureChanges.addedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.UnpublishedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.UnpublishedCulture);
            return _previousPublishCultureChanges.removedCultures?.Contains(culture) ?? false;
        }

        if (propertyName.StartsWith(ChangeTrackingPrefix.ChangedCulture))
        {
            var culture = propertyName.TrimStart(ChangeTrackingPrefix.ChangedCulture);
            return _previousPublishCultureChanges.updatedCultures?.Contains(culture) ?? false;
        }

        return base.WasPropertyDirty(propertyName);
    }

    /// <summary>
    ///     Creates a deep clone of the current entity with its identity and it's property identities reset
    /// </summary>
    /// <returns></returns>
    public IContent DeepCloneWithResetIdentities()
    {
        var clone = (Content)DeepClone();
        clone.Key = Guid.Empty;
        clone.VersionId = clone.PublishedVersionId = 0;
        clone.ResetIdentity();

        foreach (IProperty property in clone.Properties)
        {
            property.ResetIdentity();
        }

        return clone;
    }

    /// <summary>
    ///     Handles culture infos collection changes.
    /// </summary>
    private void PublishNamesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(PublishCultureInfos));

        // we don't need to handle other actions, only add/remove, however we could implement Replace and track updated cultures in _updatedCultures too
        // which would allows us to continue doing WasCulturePublished, but don't think we need it anymore
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                ContentCultureInfos? cultureInfo = e.NewItems?.Cast<ContentCultureInfos>().First();
                if (_currentPublishCultureChanges.addedCultures == null)
                {
                    _currentPublishCultureChanges.addedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (_currentPublishCultureChanges.updatedCultures == null)
                {
                    _currentPublishCultureChanges.updatedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentPublishCultureChanges.addedCultures.Add(cultureInfo.Culture);
                    _currentPublishCultureChanges.updatedCultures.Add(cultureInfo.Culture);
                    _currentPublishCultureChanges.removedCultures?.Remove(cultureInfo.Culture);
                }

                break;
            }

            case NotifyCollectionChangedAction.Remove:
            {
                // Remove listening for changes
                ContentCultureInfos? cultureInfo = e.OldItems?.Cast<ContentCultureInfos>().First();
                if (_currentPublishCultureChanges.removedCultures == null)
                {
                    _currentPublishCultureChanges.removedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentPublishCultureChanges.removedCultures.Add(cultureInfo.Culture);
                    _currentPublishCultureChanges.updatedCultures?.Remove(cultureInfo.Culture);
                    _currentPublishCultureChanges.addedCultures?.Remove(cultureInfo.Culture);
                }

                break;
            }

            case NotifyCollectionChangedAction.Replace:
            {
                // Replace occurs when an Update occurs
                ContentCultureInfos? cultureInfo = e.NewItems?.Cast<ContentCultureInfos>().First();
                if (_currentPublishCultureChanges.updatedCultures == null)
                {
                    _currentPublishCultureChanges.updatedCultures =
                        new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                }

                if (cultureInfo is not null)
                {
                    _currentPublishCultureChanges.updatedCultures.Add(cultureInfo.Culture);
                }

                break;
            }
        }
    }

    /// <summary>
    ///     Changes the <see cref="ContentType" /> for the current content object
    /// </summary>
    /// <param name="contentType">New ContentType for this content</param>
    /// <remarks>Leaves PropertyTypes intact after change</remarks>
    internal void ChangeContentType(IContentType contentType) => ChangeContentType(contentType, false);

    /// <summary>
    ///     Changes the <see cref="ContentType" /> for the current content object and removes PropertyTypes,
    ///     which are not part of the new ContentType.
    /// </summary>
    /// <param name="contentType">New ContentType for this content</param>
    /// <param name="clearProperties">Boolean indicating whether to clear PropertyTypes upon change</param>
    internal void ChangeContentType(IContentType contentType, bool clearProperties)
    {
        ChangeContentType(new SimpleContentType(contentType));

        if (clearProperties)
        {
            Properties.EnsureCleanPropertyTypes(contentType.CompositionPropertyTypes);
        }
        else
        {
            Properties.EnsurePropertyTypes(contentType.CompositionPropertyTypes);
        }

        Properties.ClearCollectionChangedEvents(); // be sure not to double add
        Properties.CollectionChanged += PropertiesChanged;
    }

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedContent = (Content)clone;

        // fixme - need to reset change tracking bits

        // if culture infos exist then deal with event bindings
        if (clonedContent._publishInfos != null)
        {
            // Clear this event handler if any
            clonedContent._publishInfos.ClearCollectionChangedEvents();

            // Manually deep clone
            clonedContent._publishInfos = (ContentCultureInfosCollection?)_publishInfos?.DeepClone();
            if (clonedContent._publishInfos is not null)
            {
                // Re-assign correct event handler
                clonedContent._publishInfos.CollectionChanged += clonedContent.PublishNamesCollectionChanged;
            }
        }

        clonedContent._currentPublishCultureChanges.updatedCultures = null;
        clonedContent._currentPublishCultureChanges.addedCultures = null;
        clonedContent._currentPublishCultureChanges.removedCultures = null;

        clonedContent._previousPublishCultureChanges.updatedCultures = null;
        clonedContent._previousPublishCultureChanges.addedCultures = null;
        clonedContent._previousPublishCultureChanges.removedCultures = null;
    }

    #region Used for change tracking

    private (HashSet<string>? addedCultures, HashSet<string>? removedCultures, HashSet<string>? updatedCultures)
        _currentPublishCultureChanges;

    private (HashSet<string>? addedCultures, HashSet<string>? removedCultures, HashSet<string>? updatedCultures)
        _previousPublishCultureChanges;

    #endregion
}
