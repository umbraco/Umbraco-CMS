using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a date and time property editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.DateTime, "Date/Time", "datepicker", ValueType = ValueTypes.DateTime, Icon="icon-time")]
    public class DateTimePropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePropertyEditor"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public DateTimePropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DateTimeValidator());
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new DateTimeConfigurationEditor();
    }
}
