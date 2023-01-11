namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQuerySettingsViewModel
{
    public required IEnumerable<string> ContentTypeAliases { get; init; }

    public required IEnumerable<TemplateQueryPropertyViewModel> Properties { get; init; }

    public required IEnumerable<TemplateQueryOperatorViewModel> Operators { get; init; }
}
