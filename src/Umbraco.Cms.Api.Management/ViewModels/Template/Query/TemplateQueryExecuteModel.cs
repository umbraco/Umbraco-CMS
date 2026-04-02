namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents the model used to execute a template query.
/// </summary>
public class TemplateQueryExecuteModel
{
    /// <summary>
    /// Gets or sets a reference to the root document, identified by its ID, used as the starting point for the template query.
    /// </summary>
    public ReferenceByIdModel? RootDocument { get; set; }

    /// <summary>
    /// Gets or sets the alias of the document type.
    /// </summary>
    public string? DocumentTypeAlias { get; set; }

    /// <summary>
    /// The collection of filters to apply when executing the template query.
    /// </summary>
    public IEnumerable<TemplateQueryExecuteFilterPresentationModel>? Filters { get; set; }

    /// <summary>
    /// Gets or sets the sort options for executing the template query.
    /// </summary>
    public TemplateQueryExecuteSortModel? Sort { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of query results to return.
    /// </summary>
    public int Take { get; set; }

}
