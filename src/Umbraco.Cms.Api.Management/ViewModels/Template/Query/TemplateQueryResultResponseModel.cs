namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the response model returned by the API for a template query result.
/// </summary>
public class TemplateQueryResultResponseModel
{
    /// <summary>
    /// Gets or sets the query expression representing the template query.
    /// </summary>
    public required string QueryExpression { get; init; }

    /// <summary>
    /// Gets or sets the collection of sample results for the template query.
    /// </summary>
    public required IEnumerable<TemplateQueryResultItemPresentationModel> SampleResults { get; init; }

    /// <summary>
    /// Gets the total number of results produced by the template query.
    /// </summary>
    public required int ResultCount { get; init; }

    /// <summary>
    /// Gets or sets the execution time of the template query, in milliseconds.
    /// </summary>
    public required long ExecutionTime { get; init; }
}
