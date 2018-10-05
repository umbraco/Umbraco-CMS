using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "Checkbox", PropertyEditorValueTypes.Integer, "boolean", IsParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new TrueFalsePreValueEditor();
        }

        internal class TrueFalsePreValueEditor : PreValueEditor
        {
            [PreValueField("default", "Default Value", "boolean")]
            public string Default { get; set; }

            public TrueFalsePreValueEditor()
            {
                Fields.Add(new PreValueField()
                {
                    Description = "",
                    Key = "showLabels",
                    Name = "Show labels",
                    View = "boolean"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "",
                    Key = "labelOn",
                    Name = "Label on text",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "",
                    Key = "labelOff",
                    Name = "Label off text",
                    View = "textstring"
                });

                Fields.Add(new PreValueField()
                {
                    Description = "",
                    Key = "hideIcons",
                    Name = "Hide icons",
                    View = "boolean"
                });
            }
        }
    }
}
