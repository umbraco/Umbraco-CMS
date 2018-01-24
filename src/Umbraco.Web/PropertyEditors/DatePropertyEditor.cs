using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.Date, "Date", "datepicker", ValueTypes.Date, Icon="icon-calendar")]
    public class DatePropertyEditor : PropertyEditor
    {
        public DatePropertyEditor(ILogger logger): base(logger)
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"format", "YYYY-MM-DD"},
                    {"pickTime", false}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        /// <inheritdoc />
        protected override ValueEditor CreateValueEditor() => new DatePropertyValueEditor(Attribute);

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor() => new DateConfigurationEditor();

        /// <summary>
        /// CUstom value editor so we can serialize with the correct date format (excluding time)
        /// and includes the date validator
        /// </summary>
        internal class DatePropertyValueEditor : ValueEditor
        {
            public DatePropertyValueEditor(ValueEditorAttribute attribute)
                : base(attribute)
            {
                Validators.Add(new DateTimeValidator());
            }

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                var date = property.GetValue().TryConvertTo<DateTime?>();
                if (date.Success == false || date.Result == null)
                {
                    return string.Empty;
                }
                //Dates will be formatted as yyyy-MM-dd
                return date.Result.Value.ToString("yyyy-MM-dd");
            }

        }
    }
}
