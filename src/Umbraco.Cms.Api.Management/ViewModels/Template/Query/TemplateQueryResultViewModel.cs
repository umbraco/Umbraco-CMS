namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryResultViewModel
{
    public required string QueryExpression { get; init; }

    public required IEnumerable<TemplateQueryResultItemViewModel> SampleResults { get; init; }

    public required int ResultCount { get; init; }

    public required long ExecutionTime { get; init; }
}
