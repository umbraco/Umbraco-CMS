namespace Umbraco.Cms.Api.Management.ViewModels.TextFiles;

public abstract class TextFileResponseModelBase : TextFileViewModelBase
{
    public string Path { get; set; } = string.Empty;

    public abstract string Type { get; }
}
