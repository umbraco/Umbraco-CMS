using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class BlockEditorConfiguration
    {
        [ConfigurationField("blocks", "Blocks", "views/propertyeditors/textarea/textarea.html")]
        public Block[] Blocks { get; set; }

        public class Block
        {
            public Udi ElementType { get; set; }

            public BlockSetting Settings { get; set; }
        }

        public class BlockSetting
        {
            public string Label { get; set; }

            public string Alias { get; set; }

            public Udi DataType { get; set; }
        }
    }
}
