using System;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// The configuration object for the Block Grid editor
    /// </summary>
    public class BlockGridConfiguration
    {
        [ConfigurationField("blocks", "Available Blocks", "views/propertyeditors/blockgrid/prevalue/blockgrid.blockconfiguration.html", Description = "Define the available blocks.")]
        public BlockGridBlockConfiguration[] Blocks { get; set; }  = null!;

        [DataContract]
        public class BlockGridBlockConfiguration
        {

            [DataMember(Name ="columnSpanOptions")]
            public BlockGridColumnSpanOption[] ColumnSpanOptions { get; set; }  = null!;
            [DataContract]
            public class BlockGridColumnSpanOption
            {
                [DataMember(Name ="columnSpan")]
                public int? ColumnSpan { get; set; }
            }

            [DataMember(Name ="areaGridColumns")]
            public int? AreaGridColumns { get; set; }

            [DataMember(Name ="areas")]
            public BlockGridAreaConfiguration[] Areas { get; set; }  = null!;

            [DataContract]
            public class BlockGridAreaConfiguration
            {
                [DataMember(Name ="key")]
                public Guid? Key { get; set; }

                [DataMember(Name ="alias")]
                public string? Alias { get; set; }
                
                [DataMember(Name ="columnSpan")]
                public int? ColumnSpan { get; set; }

                [DataMember(Name ="rowSpan")]
                public int? RowSpan { get; set; }

                [DataMember(Name ="minAllowed")]
                public int? MinAllowed { get; set; }
                
                [DataMember(Name ="maxAllowed")]
                public int? MaxAllowed { get; set; }

                [DataMember(Name ="allowAll")]
                public bool? AllowAll { get; set; }

                [DataMember(Name ="allowedTypes")]
                public BlockGridAreaConfigurationAllowedType[] AllowedTypes { get; set; }  = null!;

                [DataContract]
                public class BlockGridAreaConfigurationAllowedType
                {
                    [DataMember(Name ="elementTypeKey")]
                    public Guid? ElementTypeKey { get; set; }

                    [DataMember(Name ="minAllowed")]
                    public int? MinAllowed { get; set; }
                    
                    [DataMember(Name ="maxAllowed")]
                    public int? MaxAllowed { get; set; }

                }

            }

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
        }

        [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of blocks")]
        public NumberRange ValidationLimit { get; set; } = new NumberRange();

        [DataContract]
        public class NumberRange
        {
            [DataMember(Name ="min")]
            public int? Min { get; set; }

            [DataMember(Name ="max")]
            public int? Max { get; set; }
        }

        [ConfigurationField("useLiveEditing", "Live editing mode", "boolean", Description = "Live editing in editor overlays for live updated custom views or labels using custom expression.")]
        public bool UseLiveEditing { get; set; }

        [ConfigurationField("maxPropertyWidth", "Property editor width", "textstring", Description = "optional css overwrite, example: 800px or 100%")]
        public string? MaxPropertyWidth { get; set; }

        [ConfigurationField("gridColumns", "Grid Columns", "number", Description = "Set the number of columns for the layout")]
        public int? GridColumns { get; set; }
    }
}
