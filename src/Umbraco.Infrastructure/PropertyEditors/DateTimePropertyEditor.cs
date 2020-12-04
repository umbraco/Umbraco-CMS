using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
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
        /// <param name="loggerFactory"></param>
        public DateTimePropertyEditor(ILoggerFactory loggerFactory, IIOHelper ioHelper, IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService,localizedTextService, shortStringHelper, jsonSerializer)
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
