namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Base class for block layout items.
/// </summary>
public abstract class BlockLayoutItemBase : IBlockLayoutItem
{
    private Guid? _contentKey;
    private Guid? _settingsKey;

    private Udi? _contentUdi;
    private Udi? _settingsUdi;

    /// <inheritdoc />
    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi? ContentUdi
    {
        get => _contentUdi;
        set
        {
            if (_contentKey is not null)
            {
                return;
            }

            _contentUdi = value;
            _contentKey = (value as GuidUdi)?.Guid;
        }
    }

    /// <inheritdoc />
    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi
    {
        get => _settingsUdi;
        set
        {
            if (_settingsKey is not null)
            {
                return;
            }

            _settingsUdi = value;
            _settingsKey = (value as GuidUdi)?.Guid;
        }
    }

    /// <inheritdoc />
    public Guid ContentKey
    {
        get => _contentKey ?? throw new InvalidOperationException("ContentKey has not yet been initialized");
        set => _contentKey = value;
    }

    /// <inheritdoc />
    public Guid? SettingsKey
    {
        get => _settingsKey;
        set => _settingsKey = value;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockLayoutItemBase" /> class.
    /// </summary>
    protected BlockLayoutItemBase()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockLayoutItemBase" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    protected BlockLayoutItemBase(Udi contentUdi)
        : this((contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockLayoutItemBase" /> class.
    /// </summary>
    /// <param name="contentUdi">The content UDI.</param>
    /// <param name="settingsUdi">The settings UDI.</param>
    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    protected BlockLayoutItemBase(Udi contentUdi, Udi settingsUdi)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            (settingsUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(settingsUdi)))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockLayoutItemBase" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    protected BlockLayoutItemBase(Guid contentKey)
    {
        ContentKey = contentKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockLayoutItemBase" /> class.
    /// </summary>
    /// <param name="contentKey">The content key.</param>
    /// <param name="settingsKey">The settings key.</param>
    protected BlockLayoutItemBase(Guid contentKey, Guid settingsKey)
        : this(contentKey)
    {
        SettingsKey = settingsKey;
        SettingsUdi = new GuidUdi(Constants.UdiEntityType.Element, settingsKey);
    }

    /// <inheritdoc />
    public virtual bool ReferencesContent(Guid key)
        => ContentKey == key;

    /// <inheritdoc />
    public virtual bool ReferencesSetting(Guid key)
        => SettingsKey == key;
}
