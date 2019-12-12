using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

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
        public LabelPropertyEditor(ILogger logger, IIOHelper ioHelper)
            : base(logger)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() => new LabelPropertyValueEditor(Current.Services.DataTypeService, Current.Services.LocalizationService, Attribute);

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() => new LabelConfigurationEditor(_ioHelper);

        // provides the property value editor
        internal class LabelPropertyValueEditor : DataValueEditor
        {
            public LabelPropertyValueEditor(IDataTypeService dataTypeService, ILocalizationService localizationService, DataEditorAttribute attribute)
                : base(dataTypeService, localizationService, attribute)
            { }

            /// <inheritdoc />
            public override bool IsReadOnly => true;
        }
    }
}
