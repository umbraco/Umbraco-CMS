// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// The configuration object for the Block Grid editor
/// </summary>
public class BlockGridConfiguration
{
    [ConfigurationField("blocks", "Blocks", "views/propertyeditors/blockgrid/prevalue/blockgrid.blockconfiguration.html", Description = "Define Blocks based on Element Types.")]
    public BlockGridBlockConfiguration[] Blocks { get; set; }  = Array.Empty<BlockGridBlockConfiguration>();

    [ConfigurationField("blockGroups", "Block Groups", "views/propertyeditors/blockgrid/prevalue/blockgrid.groupconfiguration.html", HideLabel = true)]
    public BlockGridGroupConfiguration[] BlockGroups { get; set; } = Array.Empty<BlockGridGroupConfiguration>();

    [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of blocks")]
    public NumberRange ValidationLimit { get; set; } = new NumberRange();

    [ConfigurationField("useLiveEditing", "Live editing mode", "boolean", Description = "Live update content when editing in overlay")]
    public bool UseLiveEditing { get; set; }

    [ConfigurationField("maxPropertyWidth", "Editor width", "textstring", Description = "Optional css overwrite. (example: 1200px or 100%)")]
    public string? MaxPropertyWidth { get; set; }

    [ConfigurationField("gridColumns", "Grid Columns", "number", Description = "Set the number of columns for the layout. (defaults to 12)")]
    public int? GridColumns { get; set; }

    [ConfigurationField("layoutStylesheet", "Layout Stylesheet", "views/propertyeditors/blockgrid/prevalue/blockgrid.stylesheetpicker.html", Description = "Overwrite default stylesheet for layout.")]
    public string? LayoutStylesheet { get; set; }

    [ConfigurationField("createLabel", "Create Button Label", "textstring", Description = "Overwrite the root create button label")]
    public string? CreateLabel { get; set; }

    [DataContract]
    public class BlockGridBlockConfiguration : IBlockConfiguration
    {
        [DataMember(Name ="columnSpanOptions")]
        public BlockGridColumnSpanOption[] ColumnSpanOptions { get; set; }  = Array.Empty<BlockGridColumnSpanOption>();

        [DataMember(Name ="rowMaxSpan")]
        public int? RowMaxSpan { get; set; }

        [DataMember(Name ="allowAtRoot")]
        public bool AllowAtRoot { get; set; } = true;

        [DataMember(Name ="allowInAreas")]
        public bool AllowInAreas { get; set; } = true;

        [DataMember(Name ="areaGridColumns")]
        public int? AreaGridColumns { get; set; }

        [DataMember(Name = "areas")]
        public BlockGridAreaConfiguration[] Areas { get; set; } = Array.Empty<BlockGridAreaConfiguration>();

        [DataMember(Name ="backgroundColor")]
        public string? BackgroundColor { get; set; }

        [DataMember(Name ="iconColor")]
        public string? IconColor { get; set; }

        [DataMember(Name ="thumbnail")]
        public string? Thumbnail { get; set; }

        [DataMember(Name ="contentElementTypeKey")]
        public Guid ContentElementTypeKey { get; set; }

        [DataMember(Name ="settingsElementTypeKey")]
        public Guid? SettingsElementTypeKey { get; set; }

        [DataMember(Name ="view")]
        public string? View { get; set; }

        [DataMember(Name ="stylesheet")]
        public string? Stylesheet { get; set; }

        [DataMember(Name ="label")]
        public string? Label { get; set; }

        [DataMember(Name ="editorSize")]
        public string? EditorSize { get; set; }

        [DataMember(Name ="forceHideContentEditorInOverlay")]
        public bool ForceHideContentEditorInOverlay { get; set; }

        [DataMember(Name ="groupKey")]
        public string? GroupKey { get; set; }
    }

    [DataContract]
    public class BlockGridColumnSpanOption
    {
        [DataMember(Name ="columnSpan")]
        public int? ColumnSpan { get; set; }
    }

    [DataContract]
    public class BlockGridGroupConfiguration
    {

        [DataMember(Name ="key")]
        public Guid Key { get; set; }

        [DataMember(Name ="name")]
        public string? Name { get; set; }
    }

    [DataContract]
    public class NumberRange
    {
        [DataMember(Name ="min")]
        public int? Min { get; set; }

        [DataMember(Name ="max")]
        public int? Max { get; set; }
    }

    [DataContract]
    public class BlockGridAreaConfiguration
    {
        [DataMember(Name ="key")]
        public Guid Key { get; set; }

        [DataMember(Name ="alias")]
        public string? Alias { get; set; }

        [DataMember(Name ="createLabel")]
        public string? CreateLabel { get; set; }

        [DataMember(Name ="columnSpan")]
        public int? ColumnSpan { get; set; }

        [DataMember(Name ="rowSpan")]
        public int? RowSpan { get; set; }

        [DataMember(Name ="minAllowed")]
        public int? MinAllowed { get; set; }

        [DataMember(Name ="maxAllowed")]
        public int? MaxAllowed { get; set; }

        [DataMember(Name ="specifiedAllowance")]
        public BlockGridAreaConfigurationSpecifiedAllowance[] SpecifiedAllowance { get; set; } = Array.Empty<BlockGridAreaConfigurationSpecifiedAllowance>();
    }

    [DataContract]
    public class BlockGridAreaConfigurationSpecifiedAllowance
    {
        [DataMember(Name ="elementTypeKey")]
        public Guid? ElementTypeKey { get; set; }

        [DataMember(Name ="groupKey")]
        public Guid? GroupKey { get; set; }

        [DataMember(Name ="minAllowed")]
        public int? MinAllowed { get; set; }

        [DataMember(Name ="maxAllowed")]
        public int? MaxAllowed { get; set; }
    }
}
