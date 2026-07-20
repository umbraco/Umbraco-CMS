namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record DecimalExactFacet(string FieldName)
    : ExactFacet(FieldName)
{
}
