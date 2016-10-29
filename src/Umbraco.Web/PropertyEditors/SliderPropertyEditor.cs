using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.SliderAlias, "Slider", "slider", Icon="icon-navigation-horizontal")]
    public class SliderPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SliderPreValueEditor();
        }

        internal class SliderPreValueEditor : PreValueEditor
        {
            
            [PreValueField("enableRange", "Enable range", "boolean")]
            public string Range { get; set; }

            [PreValueField("orientation", "Orientation", "views/propertyeditors/slider/orientation.prevalues.html")]
            public string Orientation { get; set; }
            
            [PreValueField("initVal1", "Initial value", "number")]
            public int InitialValue { get; set; }

            [PreValueField("initVal2", "Initial value 2", "number", Description = "Used when range is enabled")]
            public int InitialValue2 { get; set; }

            [PreValueField("minVal", "Minimum value", "number")]
            public int MinimumValue { get; set; }

            [PreValueField("maxVal", "Maximum value", "number")]
            public int MaximumValue { get; set; }
            
            [PreValueField("step", "Step increments", "number")]
            public int StepIncrements { get; set; }

            [PreValueField("precision", "Precision", "number", Description = "The number of digits shown after the decimal. Defaults to the number of digits after the decimal of step value.")]
            public int Precision { get; set; }

            [PreValueField("handle", "Handle", "views/propertyeditors/slider/handle.prevalues.html", Description = "Handle shape. Default is 'round\'")]
            public string Handle { get; set; }

            [PreValueField("tooltip", "Tooltip", "views/propertyeditors/slider/tooltip.prevalues.html", Description = "Whether to show the tooltip on drag, hide the tooltip, or always show the tooltip. Accepts: 'show', 'hide', or 'always'")]
            public string Tooltip { get; set; }

            [PreValueField("tooltipSplit", "Tooltip split", "boolean", Description = "If false show one tootip if true show two tooltips one for each handler")]
            public string TooltipSplit { get; set; }

            [PreValueField("tooltipFormat", "Tooltip format", "textstring", Description = "The value wanted to be displayed in the tooltip. Use {0} and {1} for current values - {1} is only for range slider and if not using tooltip split.")]
            public string TooltipFormat { get; set; }

            [PreValueField("tooltipPosition", "Tooltip position", "textstring", Description = "Position of tooltip, relative to slider. Accepts 'top'/'bottom' for horizontal sliders and 'left'/'right' for vertically orientated sliders. Default positions are 'top' for horizontal and 'right' for vertical slider.")]
            public string TooltipPosition { get; set; }

            [PreValueField("reversed", "Reversed", "boolean", Description = "Whether or not the slider should be reversed")]
            public string Reversed { get; set; }

            [PreValueField("ticks", "Ticks", "textstring", Description = "Comma-separated values. Used to define the values of ticks. Tick marks are indicators to denote special values in the range. This option overwrites min and max options.")]
            public string Ticks { get; set; }

            [PreValueField("ticksPositions", "Ticks positions", "textstring", Description = "Comma-separated values. Defines the positions of the tick values in percentages. The first value should always be 0, the last value should always be 100 percent.")]
            public string TicksPositions { get; set; }

            [PreValueField("ticksLabels", "Ticks labels", "textstring", Description = "Comma-separated values. Defines the labels below the tick marks. Accepts HTML input.")]
            public string TicksLabels { get; set; }

            [PreValueField("ticksSnapBounds", "Ticks snap bounds", "number", Description = "Used to define the snap bounds of a tick. Snaps to the tick if value is within these bounds.")]
            public string TicksSnapBounds { get; set; }
        }
    }
}