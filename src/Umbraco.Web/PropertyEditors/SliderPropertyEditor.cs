using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a slider editor.
    /// </summary>
    [DataEditor(Constants.PropertyEditors.Aliases.Slider, "Slider", "slider", Icon = "icon-navigation-horizontal")]
    public class SliderPropertyEditor : DataEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SliderPropertyEditor"/> class.
        /// </summary>
        public SliderPropertyEditor(ILogger logger)
            : base(logger)
        { }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new SliderConfigurationEditor();
        }
    }
}
