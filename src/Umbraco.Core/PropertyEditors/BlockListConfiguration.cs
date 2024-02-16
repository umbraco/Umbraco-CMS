// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Block List editor
/// </summary>
public class BlockListConfiguration
{
    [ConfigurationField("blocks")]
    public BlockConfiguration[] Blocks { get; set; } = null!;

    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("useSingleBlockMode")]
    public bool UseSingleBlockMode { get; set; }

    public class BlockConfiguration : IBlockConfiguration
    {
        public Guid ContentElementTypeKey { get; set; }

        public Guid? SettingsElementTypeKey { get; set; }
    }

    public class NumberRange
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }
}
