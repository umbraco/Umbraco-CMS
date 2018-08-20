using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DateTimeAlias, "Date/Time", "datepicker", ValueType = PropertyEditorValueTypes.DateTime, Icon="icon-time")]
    public class DateTimePropertyEditor : PropertyEditor
    {
        public DateTimePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    //NOTE: This is very important that we do not use .Net format's there, this format
                    // is the correct format for the JS picker we are using so you cannot capitalize the HH, they need to be 'hh'
                    {"format", "YYYY-MM-DD HH:mm:ss"},
                    //a pre-value indicating if the client/server time should be offset, when set to true the date/time seen
                    // by the client will be offset with the server time.
                    // For example, this is forced to true for scheduled publishing date/time pickers
                    {"offsetTime", "0"}
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
            return new DateTimePreValueEditor();
        }
    }

    
}