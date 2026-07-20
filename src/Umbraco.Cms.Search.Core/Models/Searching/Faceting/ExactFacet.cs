namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public abstract record ExactFacet(string FieldName)
    : Facet(FieldName)
{
}
