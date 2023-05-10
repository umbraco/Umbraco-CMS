namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class RichTextGenericElement : IRichTextElement
{
    public RichTextGenericElement(string tag, Dictionary<string, object> attributes, IEnumerable<IRichTextElement> elements)
    {
        Tag = tag;
        Attributes = attributes;
        Elements = elements;
    }

    public string Tag { get; }

    public Dictionary<string, object> Attributes { get; }

    public IEnumerable<IRichTextElement> Elements { get; }
}
