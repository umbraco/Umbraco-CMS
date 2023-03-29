namespace Umbraco.Cms.Core.ContentApi;

public class SortOption
{
    public required string FieldName { get; set; }

    public Direction Direction { get; set; }

    public SortType SortType { get; set; }
}

public enum SortType
{
    Score,
    DocumentOrder,
    String,
    Int,
    Float,
    Long,
    Double
}
