namespace Umbraco.Cms.Api.Management.ViewModels.Script;

public class CreateScriptRequestModel
{
    public required string Name { get; set; }

    public string? ParentPath { get; set; }

    public string? Content { get; set; }
}
