using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
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
