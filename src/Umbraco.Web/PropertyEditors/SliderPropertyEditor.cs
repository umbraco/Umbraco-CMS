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

            [PreValueField("orientation", "Orientation", "views/propertyeditors/slider/orientation.prevalues.html")]
            public string Orientation { get; set; }

        }
    }
}