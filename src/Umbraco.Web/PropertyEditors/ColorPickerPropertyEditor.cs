using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ColorPickerAlias, "Color Picker", "colorpicker", Icon="icon-colorpicker", Group="Pickers")]
    public class ColorPickerPropertyEditor : PropertyEditor
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public ColorPickerPropertyEditor(ILogger logger, ILocalizedTextService textService) : base(logger)
        {
            _textService = textService;
        }

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// ColorListPreValueEditor uses the ValueListPreValueEditor with a custom view and controller.
        /// </remarks>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ColorListPreValueEditor(_textService);
        }

    }
}