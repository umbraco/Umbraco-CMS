// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// The configuration object for the Block Grid editor
/// </summary>
public class BlockGridConfiguration
{
    /// <summary>
    /// Gets or sets the configured blocks for the Block Grid editor.
    /// </summary>
    [ConfigurationField("blocks")]
    public BlockGridBlockConfiguration[] Blocks { get; set; } = Array.Empty<BlockGridBlockConfiguration>();

    /// <summary>
    /// Gets or sets the validation limits for the number of blocks allowed.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new NumberRange();

    /// <summary>
    /// Gets or sets the number of grid columns available in the Block Grid.
    /// </summary>
    [ConfigurationField("gridColumns")]
    public int? GridColumns { get; set; }

    /// <summary>
    /// Represents the configuration for a single block type in the Block Grid editor.
    /// </summary>
    public class BlockGridBlockConfiguration : IBlockConfiguration
    {
        /// <summary>
        /// Gets or sets the number of grid columns available within this block's areas.
        /// </summary>
        public int? AreaGridColumns { get; set; }

        /// <summary>
        /// Gets or sets the configured areas within this block.
        /// </summary>
        public BlockGridAreaConfiguration[] Areas { get; set; } = Array.Empty<BlockGridAreaConfiguration>();

        /// <summary>
        /// Gets or sets the unique key of the content element type for this block.
        /// </summary>
        public Guid ContentElementTypeKey { get; set; }

        /// <summary>
        /// Gets or sets the unique key of the settings element type for this block, if any.
        /// </summary>
        public Guid? SettingsElementTypeKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this block can be placed at the root level of the grid.
        /// </summary>
        public bool AllowAtRoot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this block can be placed within areas of other blocks.
        /// </summary>
        public bool AllowInAreas { get; set; }
    }

    /// <summary>
    /// Represents a numeric range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        /// Gets or sets the minimum value of the range.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of the range.
        /// </summary>
        public int? Max { get; set; }
    }

    /// <summary>
    /// Represents the configuration for an area within a Block Grid block.
    /// </summary>
    public class BlockGridAreaConfiguration
    {
        /// <summary>
        /// Gets or sets the unique key identifying this area.
        /// </summary>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the alias of this area.
        /// </summary>
        public string? Alias { get; set; }

        /// <summary>
        /// Gets or sets the number of columns this area spans.
        /// </summary>
        public int? ColumnSpan { get; set; }

        /// <summary>
        /// Gets or sets the number of rows this area spans.
        /// </summary>
        public int? RowSpan { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of blocks allowed in this area.
        /// </summary>
        public int? MinAllowed { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of blocks allowed in this area.
        /// </summary>
        public int? MaxAllowed { get; set; }
    }
}
