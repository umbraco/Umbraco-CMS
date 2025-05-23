namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class RichTextModel
{
    public required string Markup { get; set; }

    public required IEnumerable<ApiBlockItem> Blocks { get; set; }

    public static RichTextModel Empty() => new() { Markup = string.Empty, Blocks = Array.Empty<ApiBlockItem>() };
}
