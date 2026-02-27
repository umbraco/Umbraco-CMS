namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents an area item within a block grid layout.
/// </summary>
public class BlockGridLayoutAreaItem
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    /// <value>
    ///     The key.
    /// </value>
    public Guid Key { get; set; } = Guid.Empty;

    /// <summary>
    ///     Gets or sets the items within this area.
    /// </summary>
    /// <value>
    ///     The items.
    /// </value>
    public BlockGridLayoutItem[] Items { get; set; } = Array.Empty<BlockGridLayoutItem>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutAreaItem" /> class.
    /// </summary>
    public BlockGridLayoutAreaItem()
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockGridLayoutAreaItem" /> class.
    /// </summary>
    /// <param name="key">The key.</param>
    public BlockGridLayoutAreaItem(Guid key)
        => Key = key;

    /// <summary>
    ///     Determines whether this area contains content with the specified key.
    /// </summary>
    /// <param name="key">The content key to check.</param>
    /// <returns>
    ///     <c>true</c> if this area contains content with the specified key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsContent(Guid key)
        => Items.Any(item => item.ReferencesContent(key));

    /// <summary>
    ///     Determines whether this area contains settings with the specified key.
    /// </summary>
    /// <param name="key">The settings key to check.</param>
    /// <returns>
    ///     <c>true</c> if this area contains settings with the specified key; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsSetting(Guid key)
        => Items.Any(item => item.ReferencesSetting(key));
}
