namespace Umbraco.Cms.Core.Models.TemplateQuery;

[Obsolete("No longer used. Scheduled for removal in V17.")]
public class QueryResultModel
{
    public string? QueryExpression { get; set; }

    public IEnumerable<TemplateQueryResult>? SampleResults { get; set; }

    public int ResultCount { get; set; }

    public long ExecutionTime { get; set; }

    public int Take { get; set; }
}
