namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DateTimeOffsetExactFacet(string FieldName)
    : ExactFacet(FieldName)
{
}
