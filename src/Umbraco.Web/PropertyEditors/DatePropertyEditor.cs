using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.Date, "Date", "datepicker", ValueTypes.Date, Icon="icon-calendar")]
    public class DatePropertyEditor : PropertyEditor
    {
        public DatePropertyEditor(ILogger logger): base(logger)
        { }

        /// <inheritdoc />
        protected override IPropertyValueEditor CreateValueEditor() => new DateValueEditor(Attribute);

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor() => new DateConfigurationEditor();
    }
}
