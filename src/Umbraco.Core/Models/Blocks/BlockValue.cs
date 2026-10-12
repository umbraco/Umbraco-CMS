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

    /// <summary>
    ///     Determines whether the specified block layout alias is supported.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         By default only the layout stored under <see cref="PropertyEditorAlias" /> is supported. Override this to also
    ///         accept layouts written by other, compatible block editors - for example, a block value that can read values
    ///         stored by another editor with the same layout structure, or by an editor it has replaced.
    ///     </para>
    ///     <para>
    ///         A supported layout is read as the layout of <see cref="PropertyEditorAlias" />, and is written back under that
    ///         alias when the value is serialized. A layout stored under <see cref="PropertyEditorAlias" /> always takes
    ///         precedence over one stored under any other supported alias.
    ///     </para>
    /// </remarks>
    /// <param name="alias">The block layout alias.</param>
    /// <returns>
    ///     <c>true</c> if the specified block layout alias is supported; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool SupportsBlockLayoutAlias(string alias) => alias.Equals(PropertyEditorAlias);
}

/// <summary>
///     Represents a block value with a specific layout type.
/// </summary>
/// <typeparam name="TLayout">The type of the layout item.</typeparam>
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
