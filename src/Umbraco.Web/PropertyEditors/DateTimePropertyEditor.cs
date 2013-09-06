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
                    {"format", "yyyy-MM-dd HH:mm:ss"}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Overridden because we ONLY support Date + Time format and we don't have pre-values in the db.
        /// </summary>
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators.Add(new DateTimeValidator());

            return editor;
        }
    }
}