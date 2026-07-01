using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

/// <summary>
/// Represents a view model used for defining an operator in a template query within the management API.
/// </summary>
public class TemplateQueryOperatorViewModel
{
    /// <summary>
    /// Gets or sets the logical operator used to define the condition in the template query.
    /// </summary>
    public required Operator Operator { get; init; }

    /// <summary>
    /// Gets the collection of template query property types to which this operator can be applied.
    /// </summary>
    public required IEnumerable<TemplateQueryPropertyType> ApplicableTypes { get; init; }
}
