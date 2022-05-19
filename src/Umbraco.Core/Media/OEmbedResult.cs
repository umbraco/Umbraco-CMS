namespace Umbraco.Cms.Core.Media;

public class OEmbedResult
{
    public OEmbedStatus OEmbedStatus { get; set; }

    public bool SupportsDimensions { get; set; }

    public string? Markup { get; set; }
}
