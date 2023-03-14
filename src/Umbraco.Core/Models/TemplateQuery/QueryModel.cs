namespace Umbraco.Cms.Core.Models.TemplateQuery;

public class QueryModel
{
    public ContentTypeModel? ContentType { get; set; }

    public SourceModel? Source { get; set; }

    public IEnumerable<QueryCondition>? Filters { get; set; }

    public SortExpression? Sort { get; set; }

    public int Take { get; set; }
}
