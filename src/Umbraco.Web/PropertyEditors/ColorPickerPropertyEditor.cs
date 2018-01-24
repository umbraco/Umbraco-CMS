using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.ColorPicker, "Color Picker", "colorpicker", Icon="icon-colorpicker", Group="Pickers")]
    public class ColorPickerPropertyEditor : PropertyEditor
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public ColorPickerPropertyEditor(ILogger logger, ILocalizedTextService textService)
            : base(logger)
        {
            _textService = textService;
        }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor() => new ColorListConfigurationEditor(_textService);
    }
}
