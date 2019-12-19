using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

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
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IIOHelper _ioHelper;

        public MultiNodeTreePickerPropertyEditor(ILogger logger, IDataTypeService dataTypeService, ILocalizationService localizationService, IIOHelper ioHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService,Current.Services.TextService, Current.ShortStringHelper)
        {
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _ioHelper = ioHelper;
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiNodePickerConfigurationEditor(_ioHelper);

        protected override IDataValueEditor CreateValueEditor() => new MultiNodeTreePickerPropertyValueEditor(_dataTypeService, _localizationService, Attribute);

        public class MultiNodeTreePickerPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            public MultiNodeTreePickerPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, DataEditorAttribute attribute): base(dataTypeService, localizationService, Current.Services.TextService, Current.ShortStringHelper, attribute)
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
