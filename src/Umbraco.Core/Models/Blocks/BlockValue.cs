// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Represents a block value.
/// </summary>
public abstract class BlockValue
{
    /// <summary>
    /// Gets or sets the layout for specific property editors.
    /// </summary>
    /// <value>
    /// The layout.
    /// </value>
    public IDictionary<string, IEnumerable<IBlockLayoutItem>> Layout { get; set; } = new Dictionary<string, IEnumerable<IBlockLayoutItem>>();

    /// <summary>
    /// Gets or sets the content data.
    /// </summary>
    /// <value>
    /// The content data.
    /// </value>
    public List<BlockItemData> ContentData { get; set; } = [];

    /// <summary>
    /// Gets or sets the settings data.
    /// </summary>
    /// <value>
    /// The settings data.
    /// </value>
    public List<BlockItemData> SettingsData { get; set; } = [];

    /// <summary>
    /// Gets or sets the availability of blocks per variation.
    /// </summary>
    /// <remarks>
    /// Only applicable for block level variance.
    /// </remarks>
    public IList<BlockItemVariation> Expose { get; set; } = new List<BlockItemVariation>();

    /// <summary>
    /// Gets the property editor alias of the current layout.
    /// </summary>
    /// <value>
    /// The property editor alias of the current layout.
    /// </value>
    public abstract string PropertyEditorAlias { get; }

    [Obsolete("Will be removed in V18.")]
    public virtual bool SupportsBlockLayoutAlias(string alias) => alias.Equals(PropertyEditorAlias);
}

/// <inheritdoc />
public abstract class BlockValue<TLayout> : BlockValue
    where TLayout : IBlockLayoutItem
{
    /// <summary>
    /// Gets the layouts of the current property editor.
    /// </summary>
    /// <returns>
    /// The layouts.
    /// </returns>
    public IEnumerable<TLayout>? GetLayouts()
        => Layout.TryGetValue(PropertyEditorAlias, out IEnumerable<IBlockLayoutItem>? layouts)
            ? layouts.OfType<TLayout>()
            : null;
}
