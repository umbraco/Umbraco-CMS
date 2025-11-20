using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiRichTextElementParser
{
    IRichTextElement? Parse(string html, RichTextBlockModel? richTextBlockModel);
}
