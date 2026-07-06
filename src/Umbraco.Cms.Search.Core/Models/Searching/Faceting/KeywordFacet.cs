namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record KeywordFacet(string FieldName)
    : ExactFacet(FieldName)
{
}
