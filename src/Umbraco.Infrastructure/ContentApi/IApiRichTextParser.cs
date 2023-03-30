using Umbraco.Cms.Infrastructure.Models.ContentApi;

namespace Umbraco.Cms.Infrastructure.ContentApi;

public interface IApiRichTextParser
{
    RichTextElement? Parse(string html);
}
