using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

public class LocalLinkBlocksProcessor : LocalLinkProcessor
{
    public LocalLinkBlocksProcessor(HtmlLocalLinkParser localLinkParser, IIdKeyMap idKeyMap)
        : base(localLinkParser, idKeyMap)
    {
    }

    public bool ProcessBlocks(object? value)
    {
        if (value is not BlockValue blockValue)
        {
            return false;
        }

        bool hasChanged = false;

        foreach (BlockItemData blockItemData in blockValue.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (ProcessToEditorValue(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}
