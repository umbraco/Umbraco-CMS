using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DateTime, "Date/Time", "datepicker", ValueType = "DATETIME")]
    public class DateTimePropertyEditor : PropertyEditor
    {
        public DateTimePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, string>() ;
        }

        private IDictionary<string, string> _defaultPreVals;

        public override IDictionary<string, string> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();

            editor.Validators = new List<ValidatorBase> { new DateTimeValidator() };

            return editor;
        }
    }
}