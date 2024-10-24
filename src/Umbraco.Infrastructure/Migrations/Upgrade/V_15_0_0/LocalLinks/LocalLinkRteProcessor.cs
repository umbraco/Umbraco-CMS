using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class LocalLinkRteProcessor
{
    private readonly LocalLinkProcessor _localLinkProcessor;

    public LocalLinkRteProcessor(LocalLinkProcessor localLinkProcessor)
    {
        _localLinkProcessor = localLinkProcessor;
    }

    public bool ProcessRichText(object? value)
    {
        if (value is not RichTextEditorValue richTextValue)
        {
            return false;
        }

        bool hasChanged = false;

        var newMarkup = _localLinkProcessor.ProcessStringValue(richTextValue.Markup);
        if (newMarkup.Equals(richTextValue.Markup) == false)
        {
            hasChanged = true;
            richTextValue.Markup = newMarkup;
        }

        if (richTextValue.Blocks is null)
        {
            return hasChanged;
        }

        foreach (BlockItemData blockItemData in richTextValue.Blocks.ContentData)
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
