// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     The configuration object for the Block List editor
/// </summary>
public class BlockListConfiguration
{
    [ConfigurationField("blocks")]
    public BlockConfiguration[] Blocks { get; set; } = Array.Empty<BlockConfiguration>();

    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new();

    [ConfigurationField("useSingleBlockMode")]
    [Obsolete("Use SingleBlockPropertyEditor and its configuration instead")]
    public bool UseSingleBlockMode { get; set; }

    public class BlockConfiguration : IBlockConfiguration
    {
        public Guid ContentElementTypeKey { get; set; }

        public Guid? SettingsElementTypeKey { get; set; }

        public string? Label { get; set; }

        public string? EditorSize { get; set; }

        public string? IconColor { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Thumbnail { get; set; }

        public bool? ForceHideContentEditorInOverlay { get; set; }
    }

    public class NumberRange
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }
}
