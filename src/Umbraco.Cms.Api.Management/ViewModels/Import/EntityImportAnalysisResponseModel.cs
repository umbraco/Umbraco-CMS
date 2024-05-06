namespace Umbraco.Cms.Api.Management.ViewModels.Import;

public class EntityImportAnalysisResponseModel
{
    public string EntityType { get; set; } = string.Empty;

    public string? Alias { get; set; }

    public Guid? Key { get; set; }
}
