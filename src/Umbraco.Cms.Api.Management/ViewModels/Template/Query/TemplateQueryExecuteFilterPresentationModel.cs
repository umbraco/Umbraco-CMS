using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Presentation model for executing a filter in a template query.
/// </summary>
public class TemplateQueryExecuteFilterPresentationModel
{
    /// <summary>
    /// Gets or sets the alias of the property to filter on.
    /// </summary>
    [Required]
    public string PropertyAlias { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value used to constrain filtering during template query execution.
    /// </summary>
    [Required]
    public string ConstraintValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter operator (such as equals, contains, etc.) used to evaluate the template query condition.
    /// </summary>
    public Operator Operator { get; set; }
}
