using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

public class LocalLinkBlocksProcessor
{
    private readonly LocalLinkProcessor _localLinkProcessor;

    public LocalLinkBlocksProcessor(LocalLinkProcessor localLinkProcessor)
    {
        _localLinkProcessor = localLinkProcessor;
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
                if (_localLinkProcessor.ProcessToEditorValue(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}
