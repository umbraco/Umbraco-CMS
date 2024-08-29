namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQuerySettingsResponseModel
{
    public required IEnumerable<string> DocumentTypeAliases { get; init; }

    public required IEnumerable<TemplateQueryPropertyPresentationModel> Properties { get; init; }

    public required IEnumerable<TemplateQueryOperatorViewModel> Operators { get; init; }
}
