using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultiNodeTreePicker2Alias, "Multinode Treepicker", PropertyEditorValueTypes.Text, "contentpicker", Group="pickers", Icon="icon-page-add")]
    public class MultiNodeTreePicker2PropertyEditor : PropertyEditor
    {
        public MultiNodeTreePicker2PropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"multiPicker", "1"},
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"},
                {"idType", "udi"}
            };
        }
        
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultiNodePickerPreValueEditor();
        }

        internal IDictionary<string, object> InternalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }

        internal class MultiNodePickerPreValueEditor : PreValueEditor
        {
            public MultiNodePickerPreValueEditor()
            {
                //create the fields
                Fields.Add(new PreValueField()
                {
                    Key = "startNode",
                    View = "treesource",
                    Name = "Node type",
                    Config = new Dictionary<string, object>
                    {
                        {"idType", "udi"}
                    }
                });
                Fields.Add(new PreValueField()
                {
                    Key = "filter",
                    View = "textstring",
                    Name = "Allow items of type",
                    Description = "Separate with comma"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "minNumber",
                    View = "number",
                    Name = "Minimum number of items"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "maxNumber",
                    View = "number",
                    Name = "Maximum number of items"
                });
                Fields.Add(new PreValueField()
                {
                    Key = "showOpenButton",
                    View = "boolean",
                    Name = "Show open button (this feature is in preview!)",
                    Description = "Opens the node in a dialog"
                });
            }            

            /// <summary>
            /// This ensures the multiPicker pre-val is set based on the maxNumber of nodes set
            /// </summary>
            /// <param name="defaultPreVals"></param>
            /// <param name="persistedPreVals"></param>
            /// <returns></returns>
            /// <remarks>
            /// Due to compatibility with 7.0.0 the multiPicker pre-val might already exist in the db, but we've removed that setting in 7.0.1 so we need to detect it and if it is
            /// there, then we'll set the maxNumber to '1'
            /// </remarks>
            public override IDictionary<string, object> ConvertDbToEditor(IDictionary<string, object> defaultPreVals, PreValueCollection persistedPreVals)
            {
                var result = base.ConvertDbToEditor(defaultPreVals, persistedPreVals);

                //backwards compatibility check
                if (result.ContainsKey("multiPicker") && result["multiPicker"].ToString() == "0")
                {
                    result["maxNumber"] = "1";
                }

                //set the multiPicker val correctly depending on the maxNumber
                if (result.ContainsKey("maxNumber"))
                {
                    var asNumber = result["maxNumber"].TryConvertTo<int>();
                    if (asNumber.Success)
                    {
                        if (asNumber.Result <= 1)
                        {
                            result["multiPicker"] = "0";
                        }
                        else
                        {
                            result["multiPicker"] = "1";
                        }
                    }    
                }
                

                return result;
            }

        }
    }
}