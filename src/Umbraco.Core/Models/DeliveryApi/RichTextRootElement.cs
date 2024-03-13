namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class RichTextRootElement : IRichTextElement
{
    public RichTextRootElement(Dictionary<string, object> attributes, IEnumerable<IRichTextElement> elements, IEnumerable<ApiBlockItem> blocks)
    {
        Attributes = attributes;
        Elements = elements;
        Blocks = blocks;
    }

    public string Tag => "#root";

    public Dictionary<string, object> Attributes { get; }

    public IEnumerable<IRichTextElement> Elements { get; }

    public IEnumerable<ApiBlockItem> Blocks { get; }
}
