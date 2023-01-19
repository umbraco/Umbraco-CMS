namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryPropertyViewModel
{
    public required string Alias { get; init; }

    public required TemplateQueryPropertyType Type { get; init; }
}
