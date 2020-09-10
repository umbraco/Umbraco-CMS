using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class BlockListConfigurationEditor : ConfigurationEditor<BlockListConfiguration>
    {
        public BlockListConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {

        }

    }
}
