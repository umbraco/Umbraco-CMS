namespace Umbraco.Cms.Core.Models.DeliveryApi;

public sealed class RichTextTextElement : IRichTextElement
{
    public RichTextTextElement(string text)
        => Text = text;

    public string Text { get; }

    public string Tag => "#text";
}
