using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiRichTextParser
{
    RichTextElement? Parse(string html);
}
