namespace Umbraco.Cms.Api.Management.ViewModels.TextFiles;

public class TextFileViewModelBase
{
    public required string Name { get; set; }

    public string? Content { get; set; }
}
