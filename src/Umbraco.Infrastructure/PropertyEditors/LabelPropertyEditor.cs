using Microsoft.Extensions.Logging;
using Umbraco.Core.IO;
using Umbraco.Core.Serialization;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents a property editor for label properties.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Label,
        "Label",
        "readonlyvalue",
        Icon = "icon-readonly")]
    public class LabelPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;


        /// <summary>
        /// Initializes a new instance of the <see cref="LabelPropertyEditor"/> class.
        /// </summary>
        public LabelPropertyEditor(ILoggerFactory loggerFactory, IIOHelper ioHelper, IDataTypeService dataTypeService, ILocalizedTextService localizedTextService, ILocalizationService localizationService, IShortStringHelper shortStringHelper, IJsonSerializer jsonSerializer)
            : base(loggerFactory, dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new LabelPropertyValueEditor(DataTypeService, LocalizationService,LocalizedTextService, ShortStringHelper, Attribute, JsonSerializer);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new LabelConfigurationEditor(_ioHelper);

        // provides the property value editor
        internal class LabelPropertyValueEditor : DataValueEditor
        {
            public LabelPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, ILocalizedTextService localizedTextService, IShortStringHelper shortStringHelper, DataEditorAttribute attribute, IJsonSerializer jsonSerializer)
                : base(dataTypeService, localizationService, localizedTextService, shortStringHelper, jsonSerializer, attribute)
            { }

            /// <inheritdoc />
            public override bool IsReadOnly => true;
        }
    }
}
