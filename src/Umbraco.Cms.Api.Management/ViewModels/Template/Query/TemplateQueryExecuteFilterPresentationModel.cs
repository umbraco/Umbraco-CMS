using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryExecuteFilterPresentationModel
{
    public string PropertyAlias { get; set; } = string.Empty;

    public string ConstraintValue { get; set; } = string.Empty;

    public Operator Operator { get; set; }
}
