using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DropDownList, "Dropdown list", "dropdown")]
    public class DropDownPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            var editor = base.CreatePreValueEditor();

            editor.Fields = new List<PreValueField>
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
                };

            return editor;
        }
    }


    [PropertyEditor(Constants.PropertyEditors.Date, "Date", "DATE", "datepicker")]
    public class DatePropertyEditor : PropertyEditor
    {
        public DatePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"format", "yyyy-MM-dd"},
                    {"pickTime", false}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Overridden because we ONLY support Date (no time) format and we don't have pre-values in the db.
        /// </summary>
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override ValueEditor CreateValueEditor()
        {
            var baseEditor = base.CreateValueEditor();

            return new DateValueEditor
            {
                View = baseEditor.View
            };
        }

        /// <summary>
        /// CUstom value editor so we can serialize with the correct date format (excluding time)
        /// and includes the date validator
        /// </summary>
        private class DateValueEditor : ValueEditor
        {
            public DateValueEditor()
            {
                Validators = new List<ValidatorBase> { new DateTimeValidator() };
            }

            public override string SerializeValue(object dbValue)
            {
                var date = dbValue.TryConvertTo<DateTime?>();
                if (date.Success == false || date.Result == null)
                {
                    return string.Empty;
                }
                //Dates will be formatted as yyyy-MM-dd
                return date.Result.Value.ToString("yyyy-MM-dd");                
            }

        }
    }
}