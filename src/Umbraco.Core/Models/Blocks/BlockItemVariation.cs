namespace Umbraco.Cms.Core.Models.Blocks;

public class BlockItemVariation
{
    public BlockItemVariation()
    {
    }

    public BlockItemVariation(Guid contentKey, string? culture, string? segment)
    {
        ContentKey = contentKey;
        Culture = culture;
        Segment = segment;
    }

    public Guid ContentKey { get; set; }

    public string? Culture { get; set; }

    public string? Segment { get; set; }
}
