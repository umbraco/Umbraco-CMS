namespace Umbraco.Cms.Core.Models.Blocks;

public abstract class BlockLayoutItemBase : IBlockLayoutItem
{
    private Guid? _contentKey;
    private Guid? _settingsKey;

    private Udi? _contentUdi;
    private Udi? _settingsUdi;

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

    public Guid ContentKey
    {
        get => _contentKey ?? throw new InvalidOperationException("ContentKey has not yet been initialized");
        set => _contentKey = value;
    }

    public Guid? SettingsKey
    {
        get => _settingsKey;
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
