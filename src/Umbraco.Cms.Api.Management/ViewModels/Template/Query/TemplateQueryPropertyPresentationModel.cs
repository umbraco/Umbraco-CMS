namespace Umbraco.Cms.Api.Management.ViewModels.Template.Query;

public class TemplateQueryPropertyPresentationModel
{
    public required string Alias { get; init; }

    public required TemplateQueryPropertyType Type { get; init; }
}
