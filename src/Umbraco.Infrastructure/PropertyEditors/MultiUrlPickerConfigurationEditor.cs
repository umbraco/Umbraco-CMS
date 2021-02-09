using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MultiUrlPickerConfigurationEditor : ConfigurationEditor<MultiUrlPickerConfiguration>
    {
        public MultiUrlPickerConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
