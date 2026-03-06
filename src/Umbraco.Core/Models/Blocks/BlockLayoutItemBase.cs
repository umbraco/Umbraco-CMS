namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Base class for block layout items.
/// </summary>
public abstract class BlockLayoutItemBase : IBlockLayoutItem
{
    private Guid? _contentKey;
    private Guid? _settingsKey;

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
    /// <param name="contentKey">The content key.</param>
    protected BlockLayoutItemBase(Guid contentKey)
    {
        ContentKey = contentKey;
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
    }

    /// <inheritdoc />
    public virtual bool ReferencesContent(Guid key)
        => ContentKey == key;

    /// <inheritdoc />
    public virtual bool ReferencesSetting(Guid key)
        => SettingsKey == key;
}
