namespace Umbraco.Cms.Api.Management.ViewModels.Script;

public class ScriptViewModelBase
{
    public required string Name { get; set; }

    public string? Content { get; set; }
}
