using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;

namespace Umbraco.Cms.Core.DeliveryApi;

public interface IApiRichTextElementParser
{
    // NOTE: remember to also remove the default implementation of the method overload when this one is removed.
    [Obsolete($"Please use the overload that accepts {nameof(RichTextBlockModel)}. Will be removed in V15.")]
    IRichTextElement? Parse(string html);

    IRichTextElement? Parse(string html, RichTextBlockModel? richTextBlockModel) => null;
}
