using Umbraco.Cms.Core.Models.TemplateQuery;

namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryOperatorViewModel
{
    public required Operator Operator { get; init; }

    public required IEnumerable<TemplateQueryPropertyType> ApplicableTypes { get; init; }
}
