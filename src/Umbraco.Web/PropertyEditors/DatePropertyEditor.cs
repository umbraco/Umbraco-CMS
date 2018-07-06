using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.Date, "Date", "datepicker", ValueType = ValueTypes.Date, Icon="icon-calendar")]
    public class DatePropertyEditor : DataEditor
    {
        public DatePropertyEditor(ILogger logger): base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new DateValueEditor(Attribute);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new DateConfigurationEditor();
    }
}
