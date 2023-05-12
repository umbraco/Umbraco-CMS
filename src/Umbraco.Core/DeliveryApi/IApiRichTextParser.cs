using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiRichTextParser
{
    IRichTextElement? Parse(string html);
}
