using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the slider value editor.
    /// </summary>
    public class SliderConfigurationEditor : ConfigurationEditor<SliderConfiguration>
    {
        public SliderConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
