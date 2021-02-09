using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors.ParameterEditors
{
    /// <summary>
    /// Represents a parameter editor of some sort.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiNodeTreePicker,
        EditorType.MacroParameter,
        "Multiple Content Picker",
        "contentpicker")]
    public class MultipleContentPickerParameterEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleContentPickerParameterEditor"/> class.
        /// </summary>
        public MultipleContentPickerParameterEditor(
            ILoggerFactory loggerFactory,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            // configure
            DefaultConfiguration.Add("multiPicker", "1");
            DefaultConfiguration.Add("minNumber",0 );
            DefaultConfiguration.Add("maxNumber", 0);
        }
    }
}
