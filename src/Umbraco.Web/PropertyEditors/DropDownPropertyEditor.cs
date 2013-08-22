using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DropDownList, "Dropdown list", "dropdown")]
    public class DropDownPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            var editor = new DropDownPreValueEditor
                {
                    Fields = new List<PreValueField>
                        {
                            new PreValueField
                                {
                                    Description = "Add and remove values for the drop down list",
                                    //we're going to call this 'temp' because we are going to override the 
                                    //serialization of the pre-values to ensure that each one gets saved with it's own key 
                                    //(new db row per pre-value, thus to maintain backwards compatibility)
                                    Key = "temp",
                                    Name = ui.Text("editdatatype", "addPrevalue"),
                                    View = "Views/PropertyEditors/dropdown/dropdown.prevalue.html"
                                }
                        }
                };

            return editor;
        }
    }
}