using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Macro
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class Macro : EntityBase, IMacro
{
    private readonly IShortStringHelper _shortStringHelper;
    private List<string> _addedProperties;

    private string _alias;
    private bool _cacheByMember;
    private bool _cacheByPage;
    private int _cacheDuration;
    private bool _dontRender;
    private string _macroSource;
    private string? _name;
    private List<string> _removedProperties;
    private bool _useInEditor;

    public Macro(IShortStringHelper shortStringHelper)
    {
        _alias = string.Empty;
        _shortStringHelper = shortStringHelper;
        Properties = new MacroPropertyCollection();
        Properties.CollectionChanged += PropertiesChanged;
        _addedProperties = new List<string>();
        _removedProperties = new List<string>();
        _macroSource = string.Empty;
    }

    /// <summary>
    ///     Creates an item with pre-filled properties
    /// </summary>
    /// <param name="id"></param>
    /// <param name="key"></param>
    /// <param name="useInEditor"></param>
    /// <param name="cacheDuration"></param>
    /// <param name="alias"></param>
    /// <param name="name"></param>
    /// <param name="cacheByPage"></param>
    /// <param name="cacheByMember"></param>
    /// <param name="dontRender"></param>
    /// <param name="macroSource"></param>
    /// <param name="shortStringHelper"></param>
    public Macro(
        IShortStringHelper shortStringHelper,
        int id,
        Guid key,
        bool useInEditor,
        int cacheDuration,
        string alias,
        string? name,
        bool cacheByPage,
        bool cacheByMember,
        bool dontRender,
        string macroSource)
        : this(shortStringHelper)
    {
        Id = id;
        Key = key;
        UseInEditor = useInEditor;
        CacheDuration = cacheDuration;
        Alias = alias.ToCleanString(shortStringHelper, CleanStringType.Alias);
        Name = name;
        CacheByPage = cacheByPage;
        CacheByMember = cacheByMember;
        DontRender = dontRender;
        MacroSource = macroSource;
    }

    /// <summary>
    ///     Creates an instance for persisting a new item
    /// </summary>
    /// <param name="useInEditor"></param>
    /// <param name="cacheDuration"></param>
    /// <param name="alias"></param>
    /// <param name="name"></param>
    /// <param name="cacheByPage"></param>
    /// <param name="cacheByMember"></param>
    /// <param name="dontRender"></param>
    /// <param name="macroSource"></param>
    /// <param name="shortStringHelper"></param>
    public Macro(
        IShortStringHelper shortStringHelper,
        string alias,
        string? name,
        string macroSource,
        bool cacheByPage = false,
        bool cacheByMember = false,
        bool dontRender = true,
        bool useInEditor = false,
        int cacheDuration = 0)
        : this(shortStringHelper)
    {
        UseInEditor = useInEditor;
        CacheDuration = cacheDuration;
        Alias = alias.ToCleanString(shortStringHelper, CleanStringType.Alias);
        Name = name;
        CacheByPage = cacheByPage;
        CacheByMember = cacheByMember;
        DontRender = dontRender;
        MacroSource = macroSource;
    }

    /// <summary>
    ///     Gets or sets the alias of the Macro
    /// </summary>
    [DataMember]
    public string Alias
    {
        get => _alias;
        set => SetPropertyValueAndDetectChanges(
            value.ToCleanString(_shortStringHelper, CleanStringType.Alias),
            ref _alias!,
            nameof(Alias));
    }

    /// <summary>
    ///     Used internally to check if we need to add a section in the repository to the db
    /// </summary>
    internal IEnumerable<string> AddedProperties => _addedProperties;

    /// <summary>
    ///     Used internally to check if we need to remove  a section in the repository to the db
    /// </summary>
    internal IEnumerable<string> RemovedProperties => _removedProperties;

    public override void ResetDirtyProperties(bool rememberDirty)
    {
        base.ResetDirtyProperties(rememberDirty);

        _addedProperties.Clear();
        _removedProperties.Clear();

        foreach (IMacroProperty prop in Properties)
        {
            prop.ResetDirtyProperties(rememberDirty);
        }
    }

    /// <summary>
    ///     Gets or sets the name of the Macro
    /// </summary>
    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the Macro can be used in an Editor
    /// </summary>
    [DataMember]
    public bool UseInEditor
    {
        get => _useInEditor;
        set => SetPropertyValueAndDetectChanges(value, ref _useInEditor, nameof(UseInEditor));
    }

    /// <summary>
    ///     Gets or sets the Cache Duration for the Macro
    /// </summary>
    [DataMember]
    public int CacheDuration
    {
        get => _cacheDuration;
        set => SetPropertyValueAndDetectChanges(value, ref _cacheDuration, nameof(CacheDuration));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the Macro should be Cached by Page
    /// </summary>
    [DataMember]
    public bool CacheByPage
    {
        get => _cacheByPage;
        set => SetPropertyValueAndDetectChanges(value, ref _cacheByPage, nameof(CacheByPage));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the Macro should be Cached Personally
    /// </summary>
    [DataMember]
    public bool CacheByMember
    {
        get => _cacheByMember;
        set => SetPropertyValueAndDetectChanges(value, ref _cacheByMember, nameof(CacheByMember));
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the Macro should be rendered in an Editor
    /// </summary>
    [DataMember]
    public bool DontRender
    {
        get => _dontRender;
        set => SetPropertyValueAndDetectChanges(value, ref _dontRender, nameof(DontRender));
    }

    /// <summary>
    ///     Gets or set the path to the Partial View to render
    /// </summary>
    [DataMember]
    public string MacroSource
    {
        get => _macroSource;
        set => SetPropertyValueAndDetectChanges(value, ref _macroSource!, nameof(MacroSource));
    }

    /// <summary>
    ///     Gets or sets a list of Macro Properties
    /// </summary>
    [DataMember]
    public MacroPropertyCollection Properties { get; private set; }

    protected override void PerformDeepClone(object clone)
    {
        base.PerformDeepClone(clone);

        var clonedEntity = (Macro)clone;

        clonedEntity._addedProperties = new List<string>();
        clonedEntity._removedProperties = new List<string>();
        clonedEntity.Properties = (MacroPropertyCollection)Properties.DeepClone();

        // re-assign the event handler
        clonedEntity.Properties.CollectionChanged += clonedEntity.PropertiesChanged;
    }

    private void PropertiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Properties));

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            // listen for changes
            MacroProperty? prop = e.NewItems?.Cast<MacroProperty>().First();
            if (prop is not null)
            {
                prop.PropertyChanged += PropertyDataChanged;

                var alias = prop.Alias;

                if (_addedProperties.Contains(alias) == false)
                {
                    // add to the added props
                    _addedProperties.Add(alias);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            // remove listening for changes
            MacroProperty? prop = e.OldItems?.Cast<MacroProperty>().First();
            if (prop is not null)
            {
                prop.PropertyChanged -= PropertyDataChanged;

                var alias = prop.Alias;

                if (_removedProperties.Contains(alias) == false)
                {
                    _removedProperties.Add(alias);
                }
            }
        }
    }

    /// <summary>
    ///     When some data of a property has changed ensure our Properties flag is dirty
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PropertyDataChanged(object? sender, PropertyChangedEventArgs e) =>
        OnPropertyChanged(nameof(Properties));
}
