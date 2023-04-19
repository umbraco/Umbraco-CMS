using Umbraco.Cms.Infrastructure.Models.DeliveryApi;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

public interface IApiRichTextParser
{
    RichTextElement? Parse(string html);
}
