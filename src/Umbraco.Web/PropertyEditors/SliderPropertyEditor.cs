using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.SliderAlias, "Slider", "slider")]
    public class SliderPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SliderPreValueEditor();
        }

        internal class SliderPreValueEditor : PreValueEditor
        {
            
            [PreValueField("enableRange", "Enable range", "boolean")]
            public string Type { get; set; }
            
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

            [PreValueField("orientation", "Orientation", "views/propertyeditors/slider/orientation.prevalues.html")]
            public string Orientation { get; set; }

            [PreValueField("handle", "Handle", "views/propertyeditors/slider/handle.prevalues.html", Description = "Handle shape. Default is \"round\"")]
            public string Handle { get; set; }

            [PreValueField("reversed", "Reversed", "boolean", Description = "Whether or not the slider should be reversed")]
            public string Reversed { get; set; }

        }
    }
}