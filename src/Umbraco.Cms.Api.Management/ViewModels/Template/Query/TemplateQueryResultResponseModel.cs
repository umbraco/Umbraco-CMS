namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryResultResponseModel
{
    public required string QueryExpression { get; init; }

    public required IEnumerable<TemplateQueryResultItemPresentationModel> SampleResults { get; init; }

    public required int ResultCount { get; init; }

    public required long ExecutionTime { get; init; }
}
