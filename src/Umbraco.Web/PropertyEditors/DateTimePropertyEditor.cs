using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.DateTime, "Date/Time", "datepicker", ValueType = ValueTypes.DateTime, Icon="icon-time")]
    public class DateTimePropertyEditor : PropertyEditor
    {
        public DateTimePropertyEditor(ILogger logger): base(logger)
        { }

        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DateTimeValidator());
            return editor;
        }

        protected override ConfigurationEditor CreateConfigurationEditor() => new DateTimeConfigurationEditor();
    }
}
