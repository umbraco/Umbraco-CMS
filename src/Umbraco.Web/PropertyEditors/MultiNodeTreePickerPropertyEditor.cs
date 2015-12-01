using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MultiNodeTreePickerAlias, "Multinode Treepicker", "contentpicker", Group="pickers", Icon="icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : PropertyEditor
    {
        public MultiNodeTreePickerPropertyEditor()
        {
            _internalPreValues = new Dictionary<string, object>
            {
                {"multiPicker", "1"},
                {"showOpenButton", "0"},
                {"showEditButton", "0"},
                {"showPathOnHover", "0"}
            };
        }
        
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MultiNodePickerPreValueEditor();
        }

        private IDictionary<string, object> _internalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _internalPreValues; }
            set { _internalPreValues = value; }
        }

        internal class MultiNodePickerPreValueEditor : PreValueEditor
        {
            [PreValueField("startNode", "Node type", "treesource")]
            public string StartNode { get; set; }
            
            [PreValueField("filter", "Allow items of type", "textstring", Description = "Separate with comma")]
            public string Filter { get; set; }

            [PreValueField("minNumber", "Minimum number of items", "number")]
            public string MinNumber { get; set; }

            [PreValueField("maxNumber", "Maximum number of items", "number")]
            public string MaxNumber { get; set; }


            [PreValueField("showOpenButton", "Show open button", "boolean")]
            public string ShowOpenButton { get; set; }

            [PreValueField("showEditButton", "Show edit button (this feature is in preview!)", "boolean")]
            public string ShowEditButton { get; set; }

            [PreValueField("showPathOnHover", "Show path when hovering items", "boolean")]
            public bool ShowPathOnHover { get; set; }

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
