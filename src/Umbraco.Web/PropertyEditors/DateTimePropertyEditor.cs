using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a date and time property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.DateTime,
        "Date/Time",
        "datepicker",
        ValueType = ValueTypes.DateTime,
        Icon = "icon-time")]
    public class DateTimePropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePropertyEditor"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public DateTimePropertyEditor(ILogger logger, IIOHelper ioHelper, IShortStringHelper shortStringHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DateTimeValidator());
            return editor;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new DateTimeConfigurationEditor(_ioHelper);
    }
}
