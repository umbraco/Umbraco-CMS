// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// The configuration object for the Block Grid editor
/// </summary>
public class BlockGridConfiguration
{
    [ConfigurationField("blocks")]
    public BlockGridBlockConfiguration[] Blocks { get; set; } = Array.Empty<BlockGridBlockConfiguration>();

    [ConfigurationField("validationLimit")]
    public NumberRange ValidationLimit { get; set; } = new NumberRange();

    [ConfigurationField("gridColumns")]
    public int? GridColumns { get; set; }

    public class BlockGridBlockConfiguration : IBlockGridConfiguration
    {
        public BlockGridAreaConfiguration[] Areas { get; set; } = Array.Empty<BlockGridAreaConfiguration>();

        public Guid ContentElementTypeKey { get; set; }

        public Guid? SettingsElementTypeKey { get; set; }

        public string? Label { get; set; }

        public string? EditorSize { get; set; }

        public string? IconColor { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Thumbnail { get; set; }

        public bool? ForceHideContentEditorInOverlay { get; set; }

        public bool? DisplayInline { get; set; }

        public bool? AllowAtRoot { get; set; }

        public bool? AllowInAreas { get; set; }

        public bool? HideContentEditor { get; set; }

        public int? RowMinSpan { get; set; }

        public int? RowMaxSpan { get; set; }

        public int? AreaGridColumns { get; set; }
    }

    public class NumberRange
    {
        public int? Min { get; set; }

        public int? Max { get; set; }
    }

    public class BlockGridAreaConfiguration
    {
        public Guid Key { get; set; }

        public string? Alias { get; set; }

        public int? ColumnSpan { get; set; }

        public int? RowSpan { get; set; }

        public int? MinAllowed { get; set; }

        public int? MaxAllowed { get; set; }

        public SpecifiedAllowanceConfiguration[] SpecifiedAllowance { get; set; } = Array.Empty<SpecifiedAllowanceConfiguration>();

        public string? CreateLabel { get; set; }
    }

    public class SpecifiedAllowanceConfiguration
    {
        public int? MinAllowed { get; set; }

        public int? MaxAllowed { get; set; }

        public Guid? ElementTypeKey { get; set; }
    }
}
