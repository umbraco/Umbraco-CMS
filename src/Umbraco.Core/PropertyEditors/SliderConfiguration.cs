namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the slider value editor.
    /// </summary>
    public class SliderConfiguration
    {
        [ConfigurationField("enableRange", "Enable range", "boolean")]
        public bool EnableRange { get; set; }

        [ConfigurationField("orientation", "Orientation", "views/propertyeditors/slider/orientation.prevalues.html")]
        public string Orientation { get; set; }

        [ConfigurationField("initVal1", "Initial value", "number")]
        public int InitialValue { get; set; }

        [ConfigurationField("initVal2", "Initial value 2", "number", Description = "Used when range is enabled")]
        public int InitialValue2 { get; set; }

        [ConfigurationField("minVal", "Minimum value", "number")]
        public int MinimumValue { get; set; }

        [ConfigurationField("maxVal", "Maximum value", "number")]
        public int MaximumValue { get; set; }

        [ConfigurationField("step", "Step increments", "number")]
        public int StepIncrements { get; set; }

        [ConfigurationField("precision", "Precision", "number", Description = "The number of digits shown after the decimal. Defaults to the number of digits after the decimal of step value.")]
        public int Precision { get; set; }

        [ConfigurationField("handle", "Handle", "views/propertyeditors/slider/handle.prevalues.html", Description = "Handle shape. Default is 'round\'")]
        public string Handle { get; set; }

        [ConfigurationField("tooltip", "Tooltip", "views/propertyeditors/slider/tooltip.prevalues.html", Description = "Whether to show the tooltip on drag, hide the tooltip, or always show the tooltip. Accepts: 'show', 'hide', or 'always'")]
        public string Tooltip { get; set; }

        [ConfigurationField("tooltipSplit", "Tooltip split", "boolean", Description = "If false show one tootip if true show two tooltips one for each handler")]
        public bool TooltipSplit { get; set; } // fixme bool?

        [ConfigurationField("tooltipFormat", "Tooltip format", "textstring", Description = "The value wanted to be displayed in the tooltip. Use {0} and {1} for current values - {1} is only for range slider and if not using tooltip split.")]
        public string TooltipFormat { get; set; }

        [ConfigurationField("tooltipPosition", "Tooltip position", "textstring", Description = "Position of tooltip, relative to slider. Accepts 'top'/'bottom' for horizontal sliders and 'left'/'right' for vertically orientated sliders. Default positions are 'top' for horizontal and 'right' for vertical slider.")]
        public string TooltipPosition { get; set; }

        [ConfigurationField("reversed", "Reversed", "boolean", Description = "Whether or not the slider should be reversed")]
        public bool Reversed { get; set; } // fixme bool?

        [ConfigurationField("ticks", "Ticks", "textstring", Description = "Comma-separated values. Used to define the values of ticks. Tick marks are indicators to denote special values in the range. This option overwrites min and max options.")]
        public string Ticks { get; set; }

        [ConfigurationField("ticksPositions", "Ticks positions", "textstring", Description = "Comma-separated values. Defines the positions of the tick values in percentages. The first value should always be 0, the last value should always be 100 percent.")]
        public string TicksPositions { get; set; }

        [ConfigurationField("ticksLabels", "Ticks labels", "textstring", Description = "Comma-separated values. Defines the labels below the tick marks. Accepts HTML input.")]
        public string TicksLabels { get; set; }

        [ConfigurationField("ticksSnapBounds", "Ticks snap bounds", "number", Description = "Used to define the snap bounds of a tick. Snaps to the tick if value is within these bounds.")]
        public int TicksSnapBounds { get; set; }
    }
}
