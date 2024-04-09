using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryExecuteFilterPresentationModel
{
    [Required]
    public string PropertyAlias { get; set; } = string.Empty;

    [Required]
    public string ConstraintValue { get; set; } = string.Empty;

    public Operator Operator { get; set; }
}
