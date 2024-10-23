using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

public class LocalLinkRteProcessor : LocalLinkProcessor
{
    public LocalLinkRteProcessor(HtmlLocalLinkParser localLinkParser, IIdKeyMap idKeyMap)
        : base(localLinkParser, idKeyMap)
    {
    }

    public bool ProcessRichText(object? value)
    {
        if (value is not RichTextEditorValue richTextValue)
        {
            return false;
        }

        bool hasChanged = false;

        var newMarkup = ProcessStringValue(richTextValue.Markup);
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
                if (ProcessToEditorValue(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}
