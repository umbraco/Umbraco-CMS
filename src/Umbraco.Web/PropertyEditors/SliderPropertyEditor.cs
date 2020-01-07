using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a slider editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Slider,
        "Slider",
        "slider",
        Icon = "icon-navigation-horizontal")]
    public class SliderPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SliderPropertyEditor"/> class.
        /// </summary>
        public SliderPropertyEditor(ILogger logger, IIOHelper ioHelper, IShortStringHelper shortStringHelper)
            : base(logger, Current.Services.DataTypeService, Current.Services.LocalizationService, Current.Services.TextService, shortStringHelper)
        {
            _ioHelper = ioHelper;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new SliderConfigurationEditor(_ioHelper);
        }
    }
}
