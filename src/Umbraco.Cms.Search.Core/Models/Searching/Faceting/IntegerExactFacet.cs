namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record IntegerExactFacet(string FieldName)
    : ExactFacet(FieldName)
{
}
