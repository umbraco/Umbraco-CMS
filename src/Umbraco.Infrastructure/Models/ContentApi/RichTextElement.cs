namespace Umbraco.Cms.Infrastructure.Models.ContentApi;

public class RichTextElement
{
    public RichTextElement(string tag, string text, Dictionary<string, object> attributes, IEnumerable<RichTextElement> elements)
    {
        Tag = tag;
        Text = text;
        Attributes = attributes;
        Elements = elements;
    }

    public string Tag { get; }

    public string Text { get; }

    public Dictionary<string, object> Attributes { get; }

    public IEnumerable<RichTextElement> Elements { get; }
}
