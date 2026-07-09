// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Block List editor
/// </summary>
public class BlockListConfiguration
{
    /// <summary>
    ///     Gets or sets the available block type configurations.
    /// </summary>
    [ConfigurationField("blocks")]
    public BlockConfiguration[] Blocks { get; set; } = Array.Empty<BlockConfiguration>();

    /// <summary>
    ///     Gets or sets the validation limit for the number of blocks.
    /// </summary>
    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether single block mode is enabled.
    /// </summary>
    [ConfigurationField("useSingleBlockMode")]
    [Obsolete("Use SingleBlockPropertyEditor and its configuration instead")]
    public bool UseSingleBlockMode { get; set; }

    /// <summary>
    ///     Represents the configuration for a single block type in the Block List.
    /// </summary>
    public class BlockConfiguration : IBlockConfiguration
    {
        /// <inheritdoc />
        public Guid ContentElementTypeKey { get; set; }

        /// <inheritdoc />
        public Guid? SettingsElementTypeKey { get; set; }
    }

    /// <summary>
    ///     Represents a number range with optional minimum and maximum values.
    /// </summary>
    public class NumberRange
    {
        /// <summary>
        ///     Gets or sets the minimum number of blocks allowed.
        /// </summary>
        public int? Min { get; set; }

        /// <summary>
        ///     Gets or sets the maximum number of blocks allowed.
        /// </summary>
        public int? Max { get; set; }
    }
}
