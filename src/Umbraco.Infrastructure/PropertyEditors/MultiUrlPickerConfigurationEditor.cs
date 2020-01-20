using Umbraco.Core.IO;
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
