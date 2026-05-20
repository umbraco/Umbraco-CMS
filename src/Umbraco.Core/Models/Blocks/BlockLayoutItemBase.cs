namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Base class for block layout items.
/// </summary>
public abstract class BlockLayoutItemBase : IBlockLayoutItem
{
    /// <inheritdoc />
    public Guid Key { get; set; }

    /// <inheritdoc />
    public Guid ContentKey { get; set; }

    /// <inheritdoc />
    public Guid? SettingsKey { get; set; }

    /// <inheritdoc />
    public bool IsSharedContent { get; set; }

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

    /// <inheritdoc />
    public virtual IEnumerable<IBlockLayoutItem> GetContainedLayouts() => [];
}
