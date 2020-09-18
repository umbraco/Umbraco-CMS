using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        "Multinode Treepicker",
        "contentpicker",
        ValueType = ValueTypes.Text,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-page-add")]
    public class MultiNodeTreePickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        public MultiNodeTreePickerPropertyEditor(ILoggerFactory loggerFactory, IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IIOHelper ioHelper, IShortStringHelper shortStringHelper)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MultiNodeTreePickerPropertyValueEditor(DataTypeService, LocalizationService, LocalizedTextService, ShortStringHelper, Attribute);

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultiNodeTreePickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, DataEditorAttribute attribute)
                : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, attribute)
            {

            }

            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

                var udiPaths = asString.Split(',');
                foreach (var udiPath in udiPaths)
                    if (UdiParser.TryParse(udiPath, out var udi))
                        yield return new UmbracoEntityReference(udi);
            }
        }
    }


}
