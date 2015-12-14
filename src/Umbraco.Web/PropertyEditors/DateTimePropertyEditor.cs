using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DateTimeAlias, "Date/Time", "datepicker", ValueType = "DATETIME")]
    public class DateTimePropertyEditor : PropertyEditor
    {
        public DateTimePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    //NOTE: This is very important that we do not use .Net format's there, this format
                    // is the correct format for the JS picker we are using so you cannot capitalize the HH, they need to be 'hh'
                    {"format", "YYYY-MM-DD HH:mm:ss"}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Overridden because we ONLY support Date + Time format
        /// </summary>
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators.Add(new DateTimeValidator());

            return editor;
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new DatePreValueEditor();
        }
    }
}