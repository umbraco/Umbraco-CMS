namespace Umbraco.Cms.Core.Models.Blocks;

public abstract class BlockLayoutItemBase : IBlockLayoutItem
{
    private Guid? _contentKey;
    private Guid? _settingsKey;

    [Obsolete("Use ContentKey instead. Will be removed in V18.")]
    public Udi? ContentUdi { get; set; }

    [Obsolete("Use SettingsKey instead. Will be removed in V18.")]
    public Udi? SettingsUdi { get; set; }

    public Guid ContentKey
    {
        get
        {
            _contentKey ??= (ContentUdi as GuidUdi)?.Guid;
            return _contentKey ?? throw new InvalidOperationException("ContentKey has not yet been initialized");
        }
        set => _contentKey = value;
    }

    public Guid? SettingsKey
    {
        get
        {
            _settingsKey ??= (SettingsUdi as GuidUdi)?.Guid;
            return _settingsKey;
        }
        set => _settingsKey = value;
    }

    protected BlockLayoutItemBase()
    { }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    protected BlockLayoutItemBase(Udi contentUdi)
        : this((contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)))
    {
    }

    [Obsolete("Use constructor that accepts GUIDs instead. Will be removed in V18.")]
    protected BlockLayoutItemBase(Udi contentUdi, Udi settingsUdi)
        : this(
            (contentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(contentUdi)),
            (settingsUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(settingsUdi)))
    {
    }

    protected BlockLayoutItemBase(Guid contentKey)
    {
        ContentKey = contentKey;
        ContentUdi = new GuidUdi(Constants.UdiEntityType.Element, contentKey);
    }

    protected BlockLayoutItemBase(Guid contentKey, Guid settingsKey)
        : this(contentKey)
    {
        SettingsKey = settingsKey;
        SettingsUdi = new GuidUdi(Constants.UdiEntityType.Element, settingsKey);
    }
}
