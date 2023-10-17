namespace Umbraco.Cms.Api.Management.ViewModels.Item;

public abstract class FileItemResponseModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public abstract string Type { get; }
}
