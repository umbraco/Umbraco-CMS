namespace Umbraco.Cms.Core.ContentApi;

public class SortOption
{
    public required string FieldName { get; set; }

    public Direction Direction { get; set; }

    public FieldType FieldType { get; set; }
}

public enum FieldType
{
    String,
    Number,
    Date
}
